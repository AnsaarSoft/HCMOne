using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using DIHRMS.Custom;
using System.Data;

namespace ACHR.Screen
{
    class frm_ReHireEmp:HRMSBaseForm
    {
        #region "Global Variable Area"

        public IEnumerable<MstEmployee> Employees;
        SAPbouiCOM.Button btnSave, btnCancel;
        SAPbouiCOM.Item itxtEmpId, itxtEmpName, itxtServiceEnddt, itxtdoj, itxtCofirmdt, itxtCntrctdt, itxtconfirm, itxtbasicSal, itxtShift;
        SAPbouiCOM.EditText txtEmpId, txtEmpName, txtServiceEnddt, txtdoj, txtCofirmdt, txtCntrctdt, txtconfirm, txtbasicSal, txtShift, txtDays;
        SAPbouiCOM.ComboBox cbPayrollName, cbPaymetMode, cbMangr, cbCnfrm;
        SAPbouiCOM.DataTable dtEmp;
        SAPbouiCOM.Matrix grdReHiredEmp;
        SAPbouiCOM.Columns oColumns;
        SAPbouiCOM.Column oColumn, clNo, EmpID, EmpName;

        #endregion

        #region "B1 Events"

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            try
            {
                oForm.Freeze(true);
                InitiallizeForm();
                oForm.Freeze(false);
                oForm.ActiveItem = "txtEmpId";
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_EWD Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public override void etAfterLostFocus(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterLostFocus(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "txtEmpId":
                    LoadSelectedData(txtEmpId.Value);
                    break;
                case "txtdoj":
                    SetNoofDays();
                    break;              
                default:
                    break;
            }

        }

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            try
            {
                switch (pVal.ItemUID)
                {
                    case "btnSave":
                        ReHiredEmployee();
                        break;
                    case "2":
                        break;
                    default:
                        break;
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_ReHireEmp Function: etAfterClick Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        #endregion

        #region "Local Methods"

        public void InitiallizeForm()
        {
            try
            {

                btnCancel = oForm.Items.Item("2").Specific;
                btnSave = oForm.Items.Item("btnSave").Specific;
                //Initializing Textboxes                
                oForm.DataSources.UserDataSources.Add("txtEmpId", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtEmpId = oForm.Items.Item("txtEmpId").Specific;
                itxtEmpId = oForm.Items.Item("txtEmpId");
                txtEmpId.DataBind.SetBound(true, "", "txtEmpId");

                oForm.DataSources.UserDataSources.Add("txtEmpN", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtEmpName = oForm.Items.Item("txtEmpN").Specific;
                itxtEmpName = oForm.Items.Item("txtEmpN");
                txtEmpName.DataBind.SetBound(true, "", "txtEmpN");

                oForm.DataSources.UserDataSources.Add("txtDays", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtDays = oForm.Items.Item("txtDays").Specific; 
                txtDays.DataBind.SetBound(true, "", "txtDays");

                oForm.DataSources.UserDataSources.Add("txtSedt", SAPbouiCOM.BoDataType.dt_DATE);
                txtServiceEnddt = oForm.Items.Item("txtSedt").Specific;
                itxtServiceEnddt = oForm.Items.Item("txtSedt");
                txtServiceEnddt.DataBind.SetBound(true, "", "txtSedt");

                oForm.DataSources.UserDataSources.Add("txtdoj", SAPbouiCOM.BoDataType.dt_DATE);
                txtdoj = oForm.Items.Item("txtdoj").Specific;
                itxtdoj = oForm.Items.Item("txtdoj");
                txtdoj.DataBind.SetBound(true, "", "txtdoj");

                oForm.DataSources.UserDataSources.Add("txtCdt", SAPbouiCOM.BoDataType.dt_DATE);
                txtCofirmdt = oForm.Items.Item("txtCdt").Specific;
                itxtCofirmdt = oForm.Items.Item("txtCdt");
                txtCofirmdt.DataBind.SetBound(true, "", "txtCdt");

                oForm.DataSources.UserDataSources.Add("txtCedt", SAPbouiCOM.BoDataType.dt_DATE);
                txtCntrctdt = oForm.Items.Item("txtCedt").Specific;
                itxtCntrctdt = oForm.Items.Item("txtCedt");
                txtCntrctdt.DataBind.SetBound(true, "", "txtCedt");

                oForm.DataSources.UserDataSources.Add("txtCnfrm", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtconfirm = oForm.Items.Item("txtCnfrm").Specific;
                itxtconfirm = oForm.Items.Item("txtCnfrm");
                txtconfirm.DataBind.SetBound(true, "", "txtCnfrm");

                oForm.DataSources.UserDataSources.Add("txtBs", SAPbouiCOM.BoDataType.dt_SUM);
                txtbasicSal = oForm.Items.Item("txtBs").Specific;
                itxtbasicSal = oForm.Items.Item("txtBs");
                txtbasicSal.DataBind.SetBound(true, "", "txtBs");
                
                //Initializing ComboBxes
                cbPayrollName = oForm.Items.Item("cbPrN").Specific;
                FillPayRollNameInCombo();

                cbCnfrm = oForm.Items.Item("cbCnfrm").Specific;
                FillLovList(cbCnfrm, "ContractType");

                cbPaymetMode = oForm.Items.Item("cbPMode").Specific;
                //FillPayRollPeriodInCombo();

                cbMangr = oForm.Items.Item("cbMangr").Specific;
                FillManagersInCombo();
                FillLovList(cbPaymetMode, "PaymentMode");
                //Set Query 
                String query = @"SELECT EmpID, ISNULL(FirstName,'''') +  '' '' + ISNULL(MiddleName,'''')+ '' '' + ISNULL(LastName,'''') AS EmpName 
                                 FROM " + Program.objHrmsUI.HRMSDbName + ".dbo.MstEmployee where flgActive=''0'' ";                                                                  
                Program.objHrmsUI.addFms("frm_ReHireEmp", "txtEmpId", "-1", query);               

                InitiallizegridMatrix();

            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("InitializeFrom Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillManagersInCombo()
        {
            try
            {
                dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, dbHrPayroll.MstEmployee);
                var Data = from v in dbHrPayroll.MstEmployee
                           where v.DesignationID == 1 && v.FlgActive == true
                           select v;
                foreach (var v in Data)
                {
                    string strfullName = v.FirstName + " " + v.MiddleName + " " + v.LastName;
                    cbMangr.ValidValues.Add(Convert.ToString(v.ID), strfullName);
                }
                cbMangr.ValidValues.Add(Convert.ToString(0), Convert.ToString("None"));
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public void FillPayRollNameInCombo()
        {
            var PayrollDefination = from a in dbHrPayroll.CfgPayrollDefination select a;
            cbPayrollName.ValidValues.Add("-1", Convert.ToString("None"));
            foreach (CfgPayrollDefination PrDefination in PayrollDefination)
            {
                cbPayrollName.ValidValues.Add(Convert.ToString(PrDefination.ID), Convert.ToString(PrDefination.PayrollName));
            }
            
        }

        private void FillLovList(SAPbouiCOM.ComboBox pCombo, String TypeCode)
        {
            try
            {
                IEnumerable<MstLOVE> MartialStatus = from a in dbHrPayroll.MstLOVE where a.Type.Contains(TypeCode) select a;
                pCombo.ValidValues.Add("-1", "");
                foreach (MstLOVE One in MartialStatus)
                {
                    pCombo.ValidValues.Add(Convert.ToString(One.Code), Convert.ToString(One.Value));
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void InitiallizegridMatrix()
        {
            try
            {
                dtEmp = oForm.DataSources.DataTables.Add("Employees");                
                dtEmp.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtEmp.Columns.Add("EmpID", SAPbouiCOM.BoFieldsType.ft_Text);
                dtEmp.Columns.Add("EmpName", SAPbouiCOM.BoFieldsType.ft_Text);

                grdReHiredEmp = (SAPbouiCOM.Matrix)oForm.Items.Item("grdEmp").Specific;
                oColumns = (SAPbouiCOM.Columns)grdReHiredEmp.Columns;                

                oColumn = oColumns.Item("cl_no");
                clNo = oColumn;
                oColumn.DataBind.Bind("Employees", "No");

                oColumn = oColumns.Item("cl_empID");
                EmpID = oColumn;
                oColumn.DataBind.Bind("Employees", "EmpID");

                oColumn = oColumns.Item("cl_Name");
                EmpName = oColumn;
                oColumn.DataBind.Bind("Employees", "EmpName");
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void LoadSelectedData(String pCode)
        {
            try
            {
                if (!String.IsNullOrEmpty(pCode))
                {
                    var getEmp = (from a in dbHrPayroll.MstEmployee
                                  where a.EmpID.Contains(pCode)
                                  select a).FirstOrDefault();
                    if (getEmp != null)
                    {
                        txtEmpName.Value = getEmp.FirstName + " " + getEmp.MiddleName + " " + getEmp.LastName;
                        if (getEmp.BasicSalary != null)
                        {
                            txtbasicSal.Value = string.Format("{0:0.00}", getEmp.BasicSalary);
                        }
                        if (getEmp.TerminationDate != null)
                        {
                            if (getEmp.TerminationDate > DateTime.MinValue)
                            {
                                txtServiceEnddt.Value = Convert.ToDateTime(getEmp.TerminationDate).ToString("yyyyMMdd");
                            }
                            else
                            {
                                txtServiceEnddt.Value = "";
                            }
                        }
                    }                   
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_LoanRequest Function: LoadSelectedData Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void ReHiredEmployee()
        {
            try
            {
                string EmpId = Convert.ToString(txtEmpId.Value);
                string strEmpName = Convert.ToString(txtEmpName.Value);
                if (!string.IsNullOrEmpty(EmpId))
                {
                    var oOld = (from a in dbHrPayroll.MstEmployee where a.EmpID == EmpId select a).FirstOrDefault();
                    if (oOld != null)
                    {
                        if (!String.IsNullOrEmpty(txtdoj.Value))
                        {
                            oOld.JoiningDate = DateTime.ParseExact(txtdoj.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        }
                        if (!String.IsNullOrEmpty(txtCofirmdt.Value))
                        {
                            oOld.ConfirmationDate = DateTime.ParseExact(txtCofirmdt.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        }
                        if (!String.IsNullOrEmpty(txtCntrctdt.Value))
                        {
                            oOld.ContrEnddate = DateTime.ParseExact(txtCntrctdt.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        }
                        if (!String.IsNullOrEmpty(cbMangr.Value))
                        {
                            if (cbMangr.Value != "-1")
                            {
                                oOld.Manager = Convert.ToInt32(cbMangr.Value);
                            }
                        }
                        if (!String.IsNullOrEmpty(cbCnfrm.Value))
                        {
                            if (cbCnfrm.Value != "-1")
                            {
                                oOld.EmployeeContractType = cbCnfrm.Value;
                            }
                        }
                        if (cbPayrollName.Value != "-1")
                        {
                            oOld.PayrollID = Convert.ToInt32(cbPayrollName.Value);
                        }
                        else
                        {
                            oOld.PayrollID = null;
                        }
                        if (cbPaymetMode.Value != "-1")
                        {
                            oOld.PaymentMode = cbPaymetMode.Value.Trim();
                        }
                        else
                        {
                            oOld.PaymentMode = ""; 
                        }
                        oOld.BasicSalary = string.IsNullOrEmpty(txtbasicSal.Value) ? 0 : Convert.ToDecimal(txtbasicSal.Value);
                        oOld.FlgActive = true;
                        oOld.ResignDate = null;
                        oOld.TerminationDate = null;
                        //oOld.EmpCalender=
                        //oOld.shift=
                        dbHrPayroll.SubmitChanges();
                        int index = dtEmp.Rows.Count;
                        dtEmp.Rows.Add(index + 1);

                        dtEmp.SetValue("No", index, index + 1);
                        dtEmp.SetValue("EmpID", index, EmpId);
                        dtEmp.SetValue("EmpName", index, strEmpName);
                        grdReHiredEmp.LoadFromDataSource();
                    }                   
                }
            }
            catch (Exception Ex)
            {

                oApplication.StatusBar.SetText("Form: frm_ReHireEmp Function: InserReHiredEmployee Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void SetNoofDays()
        {
            try
            {
                if (!string.IsNullOrEmpty(txtdoj.Value) && !string.IsNullOrEmpty(txtServiceEnddt.Value))
                {
                    DateTime resighdate = DateTime.ParseExact(txtServiceEnddt.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    DateTime dateofJoining = DateTime.ParseExact(txtdoj.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                    double days = (dateofJoining - resighdate).Days;
                    //days = days + 1;
                    txtDays.Value = string.Format("{0:0.00}", days);
                }
            }
            catch (Exception ex)
            {
                
                throw;
            }
        }

        #endregion
    }
}
