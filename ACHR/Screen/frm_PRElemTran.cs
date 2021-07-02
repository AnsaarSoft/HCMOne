using System;
using System.Data;
using System.IO;
using System.Linq;
using DIHRMS;
using System.Collections;
using System.Collections.Generic;
using SAPbouiCOM;
using System.Text.RegularExpressions;

namespace ACHR.Screen
{
    partial class frm_PRElemTran : HRMSBaseForm
    {

        #region Variables

        private IEnumerable<TrnsElementPerRate> oDocCollection;
        Boolean flgEmpFrom = false, flgEmpTo = false, flgValidCall = false, flgCalculateBonus = false;
        string selEmpId = "";

        SAPbouiCOM.EditText txtDocNum, txtFileName, txtEmpFrom, txtEmpTo, txtStatus;
        SAPbouiCOM.ComboBox cmbPayroll, cmbPeriod, cmbElement;
        SAPbouiCOM.ComboBox cmbDepartment, cmbLocation, cmbDesignation, cmbBranch;
        SAPbouiCOM.Matrix grdMain;
        SAPbouiCOM.DataTable dtMain;

        SAPbouiCOM.Column clID, clSerial, clEmpCode, clEmpName, clCount, clRate, clAmount, clActive;
        SAPbouiCOM.Item itxtDocNum, itxtFileName, itxtEmpFrom, itxtEmpTo, itxtStatus;
        SAPbouiCOM.Item icmbPayroll, icmbPeriod, icmbElement;
        SAPbouiCOM.Item icmbDepartment, icmbLocation, icmbDesignation, icmbBranch;

        SAPbouiCOM.Button btnMain, btnCancel, btnEmpFrom, btnEmpTo, btnGetEmp, btnCalculate, btnPost;

        bool flgDocLoad = false, flgStatus = false, flgDocMode = false;
        double ElementValue = 0;
        string ElementFunction = string.Empty, ElementType = string.Empty;

        #endregion

        #region B1 Events

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.EnableMenu("1290", false);  // First Record
            oForm.EnableMenu("1291", false);  // Last record 
            oForm.Freeze(true);
            InitiallizeForm();
            oForm.Freeze(false);
            flgDocLoad = true;
        }

        public override void etBeforeClick(ref ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etBeforeClick(ref pVal, ref BubbleEvent);
            if (!flgDocLoad) return;
            if (pVal.ItemUID == btnGetEmp.Item.UniqueID && pVal.BeforeAction == true)
            {
                if (!ValidationGetEmployee())
                {
                    BubbleEvent = false;
                }
            }
            if (pVal.ItemUID == btnMain.Item.UniqueID && pVal.BeforeAction == true)
            {
                if (btnMain.Caption == "Add")
                {
                    if (!ValidationAddRecord())
                    {
                        BubbleEvent = false;
                    }
                }
                else if (btnMain.Caption == "Update")
                {
                    if (!ValidationUpdateRecord())
                    {
                        BubbleEvent = false;
                    }
                }
            }
            if (pVal.ItemUID == btnPost.Item.UniqueID && pVal.BeforeAction == true)
            {
                int confirm = oApplication.MessageBox("Posting is irr-reversable. Are you sure you want to post? ", 2, "Yes", "No");
                if (confirm == 2)
                {
                    BubbleEvent = false;
                }
            }
        }

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            if (!flgDocLoad) return;
            if (pVal.ItemUID == btnGetEmp.Item.UniqueID)
            {
                GetEmployees();
            }
            if (pVal.ItemUID == grdMain.Item.UniqueID && pVal.ColUID == clActive.UniqueID
                && pVal.Row == 0)
            {
                #region Alter Status Columns
                oForm.Freeze(true);
                try
                {
                    if (flgStatus)
                    {
                        for (int i = 1; i <= grdMain.RowCount; i++)
                        {
                            (grdMain.Columns.Item(clActive.UniqueID).Cells.Item(i).Specific as CheckBox).Checked = flgStatus;
                        }
                        flgStatus = false;
                    }
                    else
                    {
                        for (int i = 1; i <= grdMain.RowCount; i++)
                        {
                            (grdMain.Columns.Item(clActive.UniqueID).Cells.Item(i).Specific as CheckBox).Checked = flgStatus;
                        }
                        flgStatus = true;
                    }
                }
                catch (Exception ex)
                {
                    logger(ex);
                    MsgWarning("Change status didn't work successfully.");
                }
                oForm.Freeze(false);
                #endregion 
            }
            if (pVal.ItemUID == btnMain.Item.UniqueID)
            {
                if (!flgDocMode)
                {
                    AddRecord();
                    InitiallizeDocument();
                    GetDocumentCollection();
                }
                else
                {
                    UpdateRecord();
                }
            }
            if (pVal.ItemUID == btnPost.Item.UniqueID)
            {
                if (flgDocMode)
                {
                    
                    PostDocument();
                    InitiallizeDocument();
                }
            }
        }

        public override void etAfterCmbSelect(ref ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterCmbSelect(ref pVal, ref BubbleEvent);
            if (!flgDocLoad) return;
            if (pVal.ItemUID == cmbPayroll.Item.UniqueID)
            {
                FillComboPeriod(cmbPayroll.Value.Trim());
            }
        }

        public override void etAfterLostFocus(ref ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterLostFocus(ref pVal, ref BubbleEvent);
            if (!flgDocLoad) return;
            oForm.Freeze(true);
            if (pVal.ItemUID == grdMain.Item.UniqueID && pVal.ColUID == clCount.UniqueID && pVal.Row > 0)
            {
                #region Calculate Amount
                try
                {
                    double countvalue, ratevalue, calvalue = 0;
                    //(grdAttendance.Columns.Item("TmIn").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value.Split(':');
                    countvalue = Convert.ToDouble((grdMain.Columns.Item(clCount.UniqueID).Cells.Item(pVal.Row).Specific as EditText).Value);
                    ratevalue = Convert.ToDouble((grdMain.Columns.Item(clRate.UniqueID).Cells.Item(pVal.Row).Specific as EditText).Value);
                    if (ElementFunction == "Plus")
                    {
                        calvalue = countvalue + ratevalue;
                    }
                    else if (ElementFunction == "Minus")
                    {
                        calvalue = countvalue - ratevalue;
                    }
                    else if (ElementFunction == "Multiply")
                    {
                        calvalue = countvalue * ratevalue;
                    }
                    else if (ElementFunction == "Divide")
                    {
                        calvalue = countvalue / ratevalue;
                    }
                    (grdMain.Columns.Item(clAmount.UniqueID).Cells.Item(pVal.Row).Specific as EditText).Value = calvalue.ToString();
                    grdMain.FlushToDataSource();
                    grdMain.LoadFromDataSource();
                }
                catch (Exception ex)
                {
                    logger(ex);
                    MsgWarning("Calculation didn't end well on Row # " + pVal.Row.ToString());
                }
                #endregion
            }
            oForm.Freeze(false);
        }

        public override void getNextRecord()
        {
            base.getNextRecord();
        }

        public override void getPreviouRecord()
        {
            base.getPreviouRecord();
        }

        public override void fillFields()
        {
            base.fillFields();
            //oForm.Freeze(true);
            flgDocMode = true;
            //oForm.Freeze(false);
            FillRecord();
        }

        public override void AddNewRecord()
        {
            base.AddNewRecord();
            InitiallizeDocument();
        }

        #endregion

        #region Functions

        private void InitiallizeForm()
        {
            try
            {
                oForm.DataSources.UserDataSources.Add("txempfr", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtEmpFrom = oForm.Items.Item("txempfr").Specific;
                itxtEmpFrom = oForm.Items.Item("txempfr");
                txtEmpFrom.DataBind.SetBound(true, "", "txempfr");

                oForm.DataSources.UserDataSources.Add("txempto", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtEmpTo = oForm.Items.Item("txempto").Specific;
                itxtEmpTo = oForm.Items.Item("txempto");
                txtEmpTo.DataBind.SetBound(true, "", "txempto");

                oForm.DataSources.UserDataSources.Add("txdocnum", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txtDocNum = oForm.Items.Item("txdocnum").Specific;
                itxtDocNum = oForm.Items.Item("txdocnum");
                txtDocNum.DataBind.SetBound(true, "", "txdocnum");

                oForm.DataSources.UserDataSources.Add("txfile", SAPbouiCOM.BoDataType.dt_LONG_TEXT, 1000);
                txtFileName = oForm.Items.Item("txfile").Specific;
                itxtFileName = oForm.Items.Item("txfile");
                txtFileName.DataBind.SetBound(true, "", "txfile");

                oForm.DataSources.UserDataSources.Add("txstatus", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtStatus = oForm.Items.Item("txstatus").Specific;
                itxtStatus = oForm.Items.Item("txstatus");
                txtStatus.DataBind.SetBound(true, "", "txstatus");

                cmbPayroll = oForm.Items.Item("cbprl").Specific;
                icmbPayroll = oForm.Items.Item("cbprl");
                oForm.DataSources.UserDataSources.Add("cbprl", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cmbPayroll.DataBind.SetBound(true, "", "cbprl");

                cmbPeriod = oForm.Items.Item("cbper").Specific;
                icmbPeriod = oForm.Items.Item("cbper");
                oForm.DataSources.UserDataSources.Add("cbper", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cmbPeriod.DataBind.SetBound(true, "", "cbper");

                cmbElement = oForm.Items.Item("cbelem").Specific;
                icmbElement = oForm.Items.Item("cbelem");
                oForm.DataSources.UserDataSources.Add("cbelem", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cmbElement.DataBind.SetBound(true, "", "cbelem");


                cmbLocation = oForm.Items.Item("cbloc").Specific;
                icmbLocation = oForm.Items.Item("cbloc");
                oForm.DataSources.UserDataSources.Add("cbloc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cmbLocation.DataBind.SetBound(true, "", "cbloc");

                cmbDepartment = oForm.Items.Item("cbdept").Specific;
                icmbDepartment = oForm.Items.Item("cbdept");
                oForm.DataSources.UserDataSources.Add("cbdept", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cmbDepartment.DataBind.SetBound(true, "", "cbdept");

                cmbDesignation = oForm.Items.Item("cbdesg").Specific;
                icmbDesignation = oForm.Items.Item("cbdesg");
                oForm.DataSources.UserDataSources.Add("cbdesg", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cmbDesignation.DataBind.SetBound(true, "", "cbdesg");

                cmbBranch = oForm.Items.Item("cbbran").Specific;
                icmbBranch = oForm.Items.Item("cbbran");
                oForm.DataSources.UserDataSources.Add("cbbran", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cmbBranch.DataBind.SetBound(true, "", "cbbran");

                btnMain = oForm.Items.Item("1").Specific;
                btnCancel = oForm.Items.Item("2").Specific;
                btnGetEmp = oForm.Items.Item("btemp").Specific;
                btnCalculate = oForm.Items.Item("btcal").Specific;
                btnEmpFrom = oForm.Items.Item("btempfr").Specific;
                btnEmpTo = oForm.Items.Item("btempto").Specific;
                btnPost = oForm.Items.Item("btpost").Specific;

                grdMain = oForm.Items.Item("grdmain").Specific;
                dtMain = oForm.DataSources.DataTables.Item("dtmain");
                clID = grdMain.Columns.Item("clid");
                clID.Visible = false;
                clSerial = grdMain.Columns.Item("clser");
                clEmpCode = grdMain.Columns.Item("clcode");
                clEmpName = grdMain.Columns.Item("clname");
                clCount = grdMain.Columns.Item("clcount");
                clRate = grdMain.Columns.Item("clrate");
                clAmount = grdMain.Columns.Item("clamount");
                clActive = grdMain.Columns.Item("clactive");

                GetDocumentCollection();
                FillComboPayroll();
                FillComboPeriod(cmbPayroll.Value.Trim());
                FillComboElement();
                FillComboLocation();
                FillComboDepartment();
                FillComboDesignation();
                FillComboBranch();
                FillComboStatus();

                InitiallizeDocument();
            }
            catch (Exception ex)
            {
                logger(ex);
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

        private void FillComboElement()
        {
            try
            {
                cmbElement.ValidValues.Add("-1", "Select Value");
                var oCollection = (from a in dbHrPayroll.MstElementsPerRate
                                   select a).ToList();
                foreach (var One in oCollection)
                {
                    cmbElement.ValidValues.Add(One.Code, One.Description);
                }
                cmbElement.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private void FillComboPayroll()
        {
            try
            {
                cmbPayroll.ValidValues.Add("-1", "Select Value");
                var oCollection = (from a in dbHrPayroll.CfgPayrollDefination
                                   select a).ToList();
                foreach (var One in oCollection)
                {
                    cmbPayroll.ValidValues.Add(One.ID.ToString(), One.PayrollName);
                }
                cmbPayroll.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private void FillComboPeriod(string payroll)
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
                        cmbPeriod.ValidValues.Add(pd.ID.ToString(), pd.PeriodName.ToString());
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
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private void FillComboLocation()
        {
            try
            {
                cmbLocation.ValidValues.Add("-1", "Select Value");
                var oCollection = (from a in dbHrPayroll.MstLocation
                                   where a.FlgActive == true
                                   select a).ToList();
                foreach (var One in oCollection)
                {
                    cmbLocation.ValidValues.Add(One.Id.ToString(), One.Description);
                }
                cmbLocation.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private void FillComboDepartment()
        {
            try
            {
                cmbDepartment.ValidValues.Add("-1", "Select Value");
                var oCollection = (from a in dbHrPayroll.MstDepartment
                                   where a.FlgActive == true
                                   select a).ToList();
                foreach (var One in oCollection)
                {
                    cmbDepartment.ValidValues.Add(One.ID.ToString(), One.DeptName);
                }
                cmbDepartment.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private void FillComboDesignation()
        {
            try
            {
                cmbDesignation.ValidValues.Add("-1", "Select Value");
                var oCollection = (from a in dbHrPayroll.MstDesignation
                                   where a.FlgActive == true
                                   select a).ToList();
                foreach (var One in oCollection)
                {
                    cmbDesignation.ValidValues.Add(One.Id.ToString(), One.Description);
                }
                cmbDesignation.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private void FillComboBranch()
        {
            try
            {
                cmbBranch.ValidValues.Add("-1", "Select Value");
                var oCollection = (from a in dbHrPayroll.MstBranches
                                   where a.FlgActive == true
                                   select a).ToList();
                foreach (var One in oCollection)
                {
                    cmbBranch.ValidValues.Add(One.Id.ToString(), One.Description);
                }
                cmbBranch.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private void FillComboStatus()
        {
            try
            {
                cmbBranch.ValidValues.Add("1", "Draft");
                cmbBranch.ValidValues.Add("2", "Open");
                cmbBranch.ValidValues.Add("3", "Close");
                cmbBranch.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private string GetDocNum()
        {
            string retValue = "1";
            try
            {
                var oDoc = (from a in dbHrPayroll.TrnsElementPerRate
                            select a.DocNum).Max();
                if (oDoc == null)
                {
                    retValue = "1";
                }
                else
                {
                    retValue = ((int)oDoc.Value + 1).ToString();
                }
            }
            catch (Exception ex)
            {
                logger(ex);
                retValue = "1";
            }
            return retValue;
        }

        private void GetDocStatus()
        {
            try
            {

            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private void InitiallizeDocument()
        {
            try
            {
                txtDocNum.Value = GetDocNum();
                txtStatus.Value = "Draft";
                cmbPayroll.Select("-1", BoSearchKey.psk_ByValue);
                cmbPeriod.Select("-1", BoSearchKey.psk_ByValue);
                cmbElement.Select("-1", BoSearchKey.psk_ByValue);
                txtFileName.Value = "";
                txtEmpFrom.Value = "";
                txtEmpTo.Value = "";
                cmbLocation.Select(0, BoSearchKey.psk_Index);
                cmbDesignation.Select(0, BoSearchKey.psk_Index);
                cmbDepartment.Select(0, BoSearchKey.psk_Index);
                cmbBranch.Select(0, BoSearchKey.psk_Index);
                dtMain.Rows.Clear();
                oForm.Mode = BoFormMode.fm_ADD_MODE;
                flgDocMode = false;
                btnPost.Item.Enabled = false;
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private void GetDocumentCollection()
        {
            try
            {
                CodeIndex.Clear();
                oDocCollection = (from a in dbHrPayroll.TrnsElementPerRate
                                  select a).ToList();
                int i = 0;
                foreach (var One in oDocCollection)
                {
                    CodeIndex.Add(i, One.ID);
                    i++;
                }
                totalRecord = i;
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private bool ValidationGetEmployee()
        {
            try
            {
                string temp;
                temp = cmbPayroll.Value.Trim();
                if (!string.IsNullOrEmpty(temp))
                {
                    if (temp == "-1")
                    {
                        MsgWarning("Payroll selection is mandatory.");
                        return false;
                    }
                }
                temp = cmbPeriod.Value.Trim();
                if (!string.IsNullOrEmpty(temp))
                {
                    if (temp == "-1")
                    {
                        MsgWarning("Period selection is mandatory.");
                        return false;
                    }
                }
                temp = cmbElement.Value.Trim();
                if (string.IsNullOrEmpty(temp))
                {
                    if (temp == "-1")
                    {
                        MsgWarning("Element selection is mandatory.");
                        return false;
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                logger(ex);
                return false;
            }
        }

        private bool ValidationAddRecord()
        {
            try
            {
                grdMain.FlushToDataSource();
                string temp;
                temp = cmbPayroll.Value.Trim();
                if (!string.IsNullOrEmpty(temp))
                {
                    if (temp == "-1")
                    {
                        MsgWarning("Payroll selection is mandatory.");
                        return false;
                    }
                }
                temp = cmbPeriod.Value.Trim();
                if (!string.IsNullOrEmpty(temp))
                {
                    if (temp == "-1")
                    {
                        MsgWarning("Period selection is mandatory.");
                        return false;
                    }
                }
                temp = cmbElement.Value.Trim();
                if (string.IsNullOrEmpty(temp))
                {
                    if (temp == "-1")
                    {
                        MsgWarning("Element selection is mandatory.");
                        return false;
                    }
                }
                if (dtMain.Rows.Count == 0)
                {
                    MsgWarning("No Employee selected.");
                    return false;
                }
                else
                {
                    for (int i = 0; i < dtMain.Rows.Count; i++)
                    {
                        string linestatus;
                        double count, rate, amount;
                        count = dtMain.GetValue(clCount.DataBind.Alias, i);
                        rate = dtMain.GetValue(clRate.DataBind.Alias, i);
                        amount = dtMain.GetValue(clAmount.DataBind.Alias, i);
                        linestatus = dtMain.GetValue(clActive.DataBind.Alias, i);
                        if (count == 0 && linestatus.Trim().ToUpper() == "Y")
                        {
                            MsgWarning("Either provide count value or make employee inactive on line # " + (i + 1).ToString());
                            return false;   
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                logger(ex);
                return false;
            }
        }

        private bool ValidationUpdateRecord()
        {
            try
            {
                string temp;
                temp = cmbPayroll.Value.Trim();
                if (!string.IsNullOrEmpty(temp))
                {
                    if (temp == "-1")
                    {
                        MsgWarning("Payroll selection is mandatory.");
                        return false;
                    }
                }
                temp = cmbPeriod.Value.Trim();
                if (!string.IsNullOrEmpty(temp))
                {
                    if (temp == "-1")
                    {
                        MsgWarning("Period selection is mandatory.");
                        return false;
                    }
                }
                temp = cmbElement.Value.Trim();
                if (string.IsNullOrEmpty(temp))
                {
                    if (temp == "-1")
                    {
                        MsgWarning("Element selection is mandatory.");
                        return false;
                    }
                }
                if (dtMain.Rows.Count == 0)
                {
                    MsgWarning("No Employee selected.");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                logger(ex);
                return false;
            }
        }

        private void GetEmployees()
        {
            try
            {
                DIHRMS.Custom.DataServices ds = new DIHRMS.Custom.DataServices(dbHrPayroll, Program.objHrmsUI.HRMSDbName, oCompany.UserName, Program.objHrmsUI.logger);
                string strSql = @"
                SELECT 
	                [A].[EmpID],
	                [A].[FirstName] + ' ' + ISNULL([A].[MiddleName],'') + ' ' + ISNULL([A].[LastName], '') AS 'EmpName'
                FROM 
	                [dbo].[MstEmployee] A
                WHERE	
	                [A].[flgActive] = 1
	                AND	[A].[ResignDate] IS NULL
                                 ";
                if (cmbDepartment.Value.ToString().Trim() != "-1")
                {
                    strSql += " AND [A].[DepartmentID] = " + cmbDepartment.Value.ToString();
                }
                if (cmbLocation.Value.ToString().Trim() != "-1")
                {
                    strSql += " AND	[A].[Location] = " + cmbLocation.Value.ToString().Trim();
                }
                if (cmbDesignation.Value.ToString().Trim() != "-1")
                {
                    strSql += " AND [A].[DesignationID] = " + cmbDesignation.Value.ToString().Trim();
                }
                if (cmbBranch.Value.ToString().Trim() != "-1")
                {
                    strSql += " AND	[A].[BranchID] = " + cmbBranch.Value.ToString().Trim();
                }
                if (cmbPayroll.Value.ToString().Trim() != "-1")
                {
                    strSql += " AND	[A].[PayrollID] = " + cmbPayroll.Value.ToString().Trim();
                }
                if (!String.IsNullOrEmpty(txtEmpFrom.Value.Trim()) && !String.IsNullOrEmpty(txtEmpTo.Value.Trim()))
                {
                    Int32? FromEmpID = (from a in dbHrPayroll.MstEmployee where a.EmpID == txtEmpFrom.Value.Trim() select a.SortOrder).FirstOrDefault();
                    Int32? ToEmpID = (from a in dbHrPayroll.MstEmployee where a.EmpID == txtEmpTo.Value.Trim() select a.SortOrder).FirstOrDefault();
                    if (FromEmpID == null) FromEmpID = 0;
                    if (ToEmpID == null) ToEmpID = 100000000;
                    strSql += " and ISNULL([A].[SortOrder],0) BETWEEN " + FromEmpID + " AND " + ToEmpID + "";
                }
                System.Data.DataTable dtEmp = ds.getDataTable(strSql);
                dtMain.Rows.Clear();
                int i = 0;
                string SelectedElement = cmbElement.Value.ToString().Trim();
                var oElementLine = (from a in dbHrPayroll.MstElementsPerRate
                                    where a.FlgActive == true
                                    && a.Code == SelectedElement
                                    select a).FirstOrDefault();
                if (oElementLine != null)
                {
                    ElementValue = Convert.ToDouble(oElementLine.ElemValue);
                    ElementFunction = Convert.ToString(oElementLine.ElemFunction).Trim();
                    ElementType = Convert.ToString(oElementLine.ElemType).Trim();
                }
                foreach (DataRow dr in dtEmp.Rows)
                {
                    
                    dtMain.Rows.Add(1);
                    dtMain.SetValue(clID.DataBind.Alias, i, 0);
                    dtMain.SetValue(clSerial.DataBind.Alias, i, i + 1);
                    dtMain.SetValue(clEmpCode.DataBind.Alias, i, dr["EmpID"].ToString());
                    dtMain.SetValue(clEmpName.DataBind.Alias, i, dr["EmpName"].ToString());
                    dtMain.SetValue(clCount.DataBind.Alias, i, 0D);
                    dtMain.SetValue(clRate.DataBind.Alias, i, ElementValue);
                    dtMain.SetValue(clAmount.DataBind.Alias, i, 0D);
                    dtMain.SetValue(clActive.DataBind.Alias, i, "Y");
                    i++;
                }
                grdMain.LoadFromDataSource();
                grdMain.AutoResizeColumns();
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private void AddRecord()
        {
            try
            {

                TrnsElementPerRate oDoc = new TrnsElementPerRate();
                dbHrPayroll.TrnsElementPerRate.InsertOnSubmit(oDoc);
                oDoc.DocNum = Convert.ToInt32(GetDocNum());
                oDoc.DocStatus = "Open";
                oDoc.PayrollLink = Convert.ToInt32(cmbPayroll.Value.ToString().Trim());
                oDoc.PeriodLink = Convert.ToInt32(cmbPeriod.Value.ToString().Trim());
                oDoc.ProcessOn = Convert.ToString(cmbElement.Value.ToString().Trim());
                oDoc.CreatedBy = oCompany.UserName;
                oDoc.UpdatedBy = oCompany.UserName;
                oDoc.CreateDate = DateTime.Now;
                oDoc.UpdateDate = DateTime.Now;
                for (int i = 0; i < dtMain.Rows.Count; i++)
                {
                    int lineid;
                    string empcode, linestatus;
                    double count, rate, amount;
                    lineid = dtMain.GetValue(clID.DataBind.Alias, i);
                    empcode = dtMain.GetValue(clEmpCode.DataBind.Alias, i);
                    count = dtMain.GetValue(clCount.DataBind.Alias, i);
                    rate = dtMain.GetValue(clRate.DataBind.Alias, i);
                    amount = dtMain.GetValue(clAmount.DataBind.Alias, i);
                    linestatus = dtMain.GetValue(clActive.DataBind.Alias, i);
                    if (count == 0) continue;
                    if (linestatus.Trim().ToUpper() == "N") continue;
                    if (lineid == 0)
                    {
                        var oEmp = (from a in dbHrPayroll.MstEmployee
                                    where a.EmpID == empcode
                                    select a).FirstOrDefault();
                        TrnsElementPerRateDetail oLine = new TrnsElementPerRateDetail();
                        oDoc.TrnsElementPerRateDetail.Add(oLine);
                        oLine.EmpID = oEmp.ID;
                        oLine.Count = Convert.ToDecimal(count);
                        oLine.Rate = Convert.ToDecimal(rate);
                        oLine.Amount = Convert.ToDecimal(amount);
                    }
                }
                dbHrPayroll.SubmitChanges();
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private void UpdateRecord()
        {
            try
            {

            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private void FillRecord()
        {
            try
            {
                if (CodeIndex.Count == 0) return;
                string value = CodeIndex[currentRecord].ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    var oDoc = (from a in dbHrPayroll.TrnsElementPerRate
                                where a.ID.ToString() == value
                                select a).FirstOrDefault();
                    if (oDoc == null) return;

                    txtDocNum.Value = oDoc.DocNum.ToString();
                    cmbPayroll.Select(oDoc.PayrollLink.ToString(), BoSearchKey.psk_ByValue);
                    //FillComboPeriod(cmbPayroll.Value.ToString().Trim());
                    cmbPeriod.Select(oDoc.PeriodLink.ToString(), BoSearchKey.psk_ByValue);
                    cmbElement.Select(oDoc.ProcessOn, BoSearchKey.psk_ByValue);
                    if (oDoc.DocStatus == "Open")
                    {
                        txtStatus.Value = oDoc.DocStatus;
                        //btnPost.Item.Enabled = true;
                        btnPost.Item.Visible = true;   
                    }
                    else
                    {
                        txtStatus.Value = oDoc.DocStatus;
                        //btnPost.Item.Enabled = false;
                        btnPost.Item.Visible = false;
                    }
                    dtMain.Rows.Clear();
                    int i = 0;
                    foreach(var One in oDoc.TrnsElementPerRateDetail)
                    {
                        var oEmp = (from a in dbHrPayroll.MstEmployee
                                    where a.ID == One.EmpID
                                    select a).FirstOrDefault();
                        dtMain.Rows.Add(1);
                        dtMain.SetValue(clID.DataBind.Alias, i, One.ID);
                        dtMain.SetValue(clSerial.DataBind.Alias, i, i + 1);
                        dtMain.SetValue(clEmpCode.DataBind.Alias, i, oEmp.EmpID);
                        dtMain.SetValue(clEmpName.DataBind.Alias, i, oEmp.FirstName + " " + oEmp.MiddleName + " " + oEmp.LastName);
                        dtMain.SetValue(clCount.DataBind.Alias, i, Convert.ToDouble(One.Count));
                        dtMain.SetValue(clRate.DataBind.Alias, i, Convert.ToDouble(One.Rate));
                        dtMain.SetValue(clAmount.DataBind.Alias, i, Convert.ToDouble(One.Amount));
                        dtMain.SetValue(clActive.DataBind.Alias, i, "Y");
                        i++;
                    }
                    grdMain.LoadFromDataSource();
                    grdMain.AutoResizeColumns();
                    oForm.Mode = BoFormMode.fm_OK_MODE;
                }
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private void PostDocument()
        {
            try
            {
                if (!string.IsNullOrEmpty(txtDocNum.Value.ToString().Trim()))
                {
                    var oDoc = (from a in dbHrPayroll.TrnsElementPerRate
                                where a.DocNum.ToString() == txtDocNum.Value.ToString().Trim()
                                select a).FirstOrDefault();
                    if (oDoc == null) return;
                    if (oDoc.DocStatus == "Closed")
                    {
                        MsgWarning("Document already closed you can't repost.");
                        return;
                    }
                    var oElemPerRate = (from a in dbHrPayroll.MstElementsPerRate
                                        where a.Code == oDoc.ProcessOn
                                        select a).FirstOrDefault();
                    if (oElemPerRate == null) return;
                    var oElement = (from a in dbHrPayroll.MstElements
                                    where a.Id == oElemPerRate.PayThrough
                                    select a).FirstOrDefault();
                    if (oElement == null) return;
                    var oPayrollLink = (from a in dbHrPayroll.MstElementLink
                                        where a.ElementID == oElement.Id
                                        select a).FirstOrDefault();
                    if (oPayrollLink == null)
                    {
                        MsgWarning("Assign element to selected payroll.");
                        return;
                    }
                    int i = 0;
                    foreach(var One in oDoc.TrnsElementPerRateDetail)
                    {
                        int lineid;
                        string empcode;
                        double count, rate, amount;
                        lineid = dtMain.GetValue(clID.DataBind.Alias, i);
                        empcode = dtMain.GetValue(clEmpCode.DataBind.Alias, i);
                        count = dtMain.GetValue(clCount.DataBind.Alias, i);
                        rate = dtMain.GetValue(clRate.DataBind.Alias, i);
                        amount = dtMain.GetValue(clAmount.DataBind.Alias, i);

                        var oHeadElement = (from a in dbHrPayroll.TrnsEmployeeElement
                                            where a.MstEmployee.EmpID == empcode
                                            select a).FirstOrDefault();
                        if (oHeadElement == null) return;
                        TrnsEmployeeElementDetail oDetailElement = null;
                        oDetailElement = (from a in dbHrPayroll.TrnsEmployeeElementDetail
                                              where a.EmpElmtId == oHeadElement.Id
                                              && a.ElementId == oElement.Id
                                              && a.FlgOneTimeConsumed == false
                                              select a).FirstOrDefault();
                        if (oDetailElement == null)
                        {
                            oDetailElement = new TrnsEmployeeElementDetail();
                            oHeadElement.TrnsEmployeeElementDetail.Add(oDetailElement);
                            oDetailElement.MstElements = oElement;
                            oDetailElement.StartDate = oElement.StartDate;
                            oDetailElement.EndDate = oElement.EndDate;
                            oDetailElement.RetroAmount = 0;
                            oDetailElement.FlgRetro = false;
                            oDetailElement.FlgPayroll = false;
                            oDetailElement.FlgModified = false;
                            oDetailElement.FlgTaxable = false;
                            oDetailElement.FlgStandard = false;
                            oDetailElement.FlgActive = true;
                            oDetailElement.FlgOneTimeConsumed = false;
                            oDetailElement.CreateDate = DateTime.Now;
                            oDetailElement.UpdateDate = DateTime.Now;
                            oDetailElement.UserId = oCompany.UserName;
                            oDetailElement.UpdatedBy = oCompany.UserName;
                            oDetailElement.ElementType = "Ear";
                            oDetailElement.ValueType = "FIX";
                            oDetailElement.Value = Convert.ToDecimal(amount);
                            oDetailElement.Amount = Convert.ToDecimal(amount);
                            oDetailElement.EmpContr = 0;
                            oDetailElement.EmplrContr = 0;
                            oDetailElement.PeriodId = oDoc.PeriodLink;
                        }
                        else
                        {
                            oDetailElement.StartDate = oElement.StartDate;
                            oDetailElement.EndDate = oElement.EndDate;
                            oDetailElement.RetroAmount = 0;
                            oDetailElement.FlgRetro = false;
                            oDetailElement.FlgPayroll = false;
                            oDetailElement.FlgModified = false;
                            oDetailElement.FlgTaxable = false;
                            oDetailElement.FlgStandard = false;
                            oDetailElement.FlgActive = true;
                            oDetailElement.FlgOneTimeConsumed = false;
                            oDetailElement.UpdateDate = DateTime.Now;
                            oDetailElement.UpdatedBy = oCompany.UserName;
                            oDetailElement.ElementType = "Ear";
                            oDetailElement.ValueType = "FIX";
                            oDetailElement.Value = Convert.ToDecimal(amount);
                            oDetailElement.Amount = Convert.ToDecimal(amount);
                            oDetailElement.EmpContr = 0;
                            oDetailElement.EmplrContr = 0;
                            oDetailElement.PeriodId = oDoc.PeriodLink;
                        }
                        i++;
                    }
                    oDoc.DocStatus = "Closed";
                    dbHrPayroll.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        #endregion

    }

}
