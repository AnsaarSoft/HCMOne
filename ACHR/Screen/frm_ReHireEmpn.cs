using System;
using System.Data;
using System.Collections;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using DIHRMS.Custom;

namespace ACHR.Screen
{
    class frm_ReHireEmpn : HRMSBaseForm
    {
        #region Variable

        SAPbouiCOM.EditText txtEmpCode, txtEmpName, txtLocationOld, txtDepartmentOld, txtDesignationOld, txtBranchOld;
        SAPbouiCOM.EditText txtManagerOld, txtBasicSalaryOld, txtGrossSalaryOld, txtDocNo, txtDocDate, txtJoiningDtOld;
        SAPbouiCOM.EditText txtResignDt, txtTerminationDt, txtBasicSalaryNew, txtJoiningDtNew, txtManagerNew;
        SAPbouiCOM.ComboBox cbLocationNew, cbDepartmentNew, cbDesignationNew, cbBranchNew ;
        SAPbouiCOM.Item itxtEmpCode, itxtEmpName, itxtLocationOld, itxtDepartmentOld, itxtDesignationOld, itxtBranchOld;
        SAPbouiCOM.Item itxtManagerOld, itxtBasicSalaryOld, itxtGrossSalaryOld, itxtDocNo, itxtDocDate, itxtJoiningDtOld;
        SAPbouiCOM.Item itxtResignDt, itxtTerminationDt, itxtBasicSalaryNew, itxtJoiningDtNew, itxtManagerNew;
        SAPbouiCOM.Item icbLocationNew, icbDepartmentNew, icbDesignationNew, icbBranchNew ;
        SAPbouiCOM.Button btnMain, btnCancel, btnReHire, btnEmpPick, btnMngPick;
        SAPbouiCOM.Item ibtnMain, ibtnCancel, ibtnReHire, ibtnEmpPick, ibtnMngPick;

        public Hashtable CodeIndex = new Hashtable();
        IEnumerable<TrnsEmployeeReHire> oDocuments = null;
        Boolean flgEmpSelect, flgManagerSelect;
        Int32 OpenDocID = 0;
        IEnumerable<MstEmployee> oEmployees = null;

        #endregion

        #region B1 Events

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            try
            {
                oForm.Freeze(true);
                InitiallizeForm();
                oForm.Freeze(false);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("CreateForm Base : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "1":
                    //DoAction();
                    break;
                case "3":
                    flgEmpSelect = true;
                    flgManagerSelect = false;
                    OpenNewSearchForm();
                    break;
                case "4":
                    flgEmpSelect = false;
                    flgManagerSelect = true;
                    OpenNewSearchFormManager();
                    break;
                case "2":
                    oForm.Close();
                    break;
                case "5":
                    ReHireEmployeeMaster();
                    break;
            }
        }

        public override void etBeforeClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etBeforeClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "1":
                    DoAction();
                    break;
            }
        }

        public override void etFormAfterActivate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etFormAfterActivate(ref pVal, ref BubbleEvent);
            if (txtEmpCode == null)
            {
                Program.EmpID = string.Empty;
                return;
            }
            if (Program.EmpID == string.Empty)
            {
                return;
            }
            if (!string.IsNullOrEmpty(Program.EmpID) && flgEmpSelect && !flgManagerSelect)
            {
                if (Program.EmpID != txtEmpCode.Value.Trim())
                {
                    SetEmpValues();
                }
            }
            if (!string.IsNullOrEmpty(Program.EmpID) && !flgEmpSelect && flgManagerSelect)
            {
                if (Program.EmpID != txtManagerNew.Value.Trim())
                {
                    SetManagerEmployee();
                }
            }
        }

        public override void getNextRecord()
        {
            base.getNextRecord();
        }

        public override void getPreviouRecord()
        {
            base.getPreviouRecord();
        }

        public override void getLastRecord()
        {
            base.getLastRecord();
        }

        public override void getFirstRecord()
        {
            base.getFirstRecord();
        }

        public override void AddNewRecord()
        {
            base.AddNewRecord();
            ClearRecord();
        }

        public override void fillFields()
        {
            base.fillFields();
            oForm.Freeze(true);
            try
            {
                TrnsEmployeeReHire oDocReHire = oDocuments.ElementAt<TrnsEmployeeReHire>(currentRecord);
                if (oDocReHire != null)
                {
                    FillRecord(oDocReHire);
                }
            }
            catch
            {
            }
            oForm.Freeze(false);
        }
        public override void FindRecordMode()
        {
            base.FindRecordMode();

            OpenNewSearchForm();
            oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            flgEmpSelect = true;
            flgManagerSelect = false;
        }
        #endregion

        #region Function

        private void InitiallizeForm()
        {
            try
            {
                txtEmpCode = oForm.Items.Item("txEmpId").Specific;
                itxtEmpCode = oForm.Items.Item("txEmpId");
                oForm.DataSources.UserDataSources.Add("txEmpId", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                txtEmpCode.DataBind.SetBound(true, "", "txEmpId");

                txtEmpName = oForm.Items.Item("txEmpName").Specific;
                itxtEmpName = oForm.Items.Item("txEmpName");
                oForm.DataSources.UserDataSources.Add("txEmpName", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                txtEmpName.DataBind.SetBound(true, "", "txEmpName");
                

                txtLocationOld = oForm.Items.Item("txLoc").Specific;
                itxtLocationOld = oForm.Items.Item("txLoc");
                oForm.DataSources.UserDataSources.Add("txLoc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                txtLocationOld.DataBind.SetBound(true, "", "txLoc");
                

                txtDepartmentOld = oForm.Items.Item("txDept").Specific;
                itxtDepartmentOld = oForm.Items.Item("txDept");
                oForm.DataSources.UserDataSources.Add("txDept", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                txtDepartmentOld.DataBind.SetBound(true, "", "txDept");
                

                txtDesignationOld = oForm.Items.Item("txDesig").Specific;
                itxtDesignationOld = oForm.Items.Item("txDesig");
                oForm.DataSources.UserDataSources.Add("txDesig", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                txtDesignationOld.DataBind.SetBound(true, "", "txDesig");
                

                txtBranchOld = oForm.Items.Item("txBranch").Specific;
                itxtBranchOld = oForm.Items.Item("txBranch");
                oForm.DataSources.UserDataSources.Add("txBranch", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                txtBranchOld.DataBind.SetBound(true, "", "txBranch");
                

                //txManager
                txtManagerOld = oForm.Items.Item("txManager").Specific;
                itxtManagerOld = oForm.Items.Item("txManager");
                oForm.DataSources.UserDataSources.Add("txManager", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                txtManagerOld.DataBind.SetBound(true, "", "txManager");


                txtManagerNew = oForm.Items.Item("txMngN").Specific;
                itxtManagerNew = oForm.Items.Item("txMngN");
                oForm.DataSources.UserDataSources.Add("txMngN", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                txtManagerNew.DataBind.SetBound(true, "", "txMngN");

                //txBS
                txtBasicSalaryOld = oForm.Items.Item("txBS").Specific;
                itxtBasicSalaryOld = oForm.Items.Item("txBS");
                oForm.DataSources.UserDataSources.Add("txBS", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                txtBasicSalaryOld.DataBind.SetBound(true, "", "txBS");
                

                //txGS
                txtGrossSalaryOld = oForm.Items.Item("txGS").Specific;
                itxtGrossSalaryOld = oForm.Items.Item("txGS");
                oForm.DataSources.UserDataSources.Add("txGS", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                txtGrossSalaryOld.DataBind.SetBound(true, "", "txGS");
                

                //txTerDt
                txtTerminationDt = oForm.Items.Item("txTerDt").Specific;
                itxtTerminationDt = oForm.Items.Item("txTerDt");
                oForm.DataSources.UserDataSources.Add("txTerDt", SAPbouiCOM.BoDataType.dt_DATE);
                txtTerminationDt.DataBind.SetBound(true, "", "txTerDt");
                

                //txResignDt
                txtResignDt = oForm.Items.Item("txResignDt").Specific;
                itxtResignDt = oForm.Items.Item("txResignDt");
                oForm.DataSources.UserDataSources.Add("txResignDt", SAPbouiCOM.BoDataType.dt_DATE);
                txtResignDt.DataBind.SetBound(true, "", "txResignDt");
                

                //txJoinDt
                txtJoiningDtOld = oForm.Items.Item("txJoinDt").Specific;
                itxtJoiningDtOld = oForm.Items.Item("txJoinDt");
                oForm.DataSources.UserDataSources.Add("txJoinDt", SAPbouiCOM.BoDataType.dt_DATE);
                txtJoiningDtOld.DataBind.SetBound(true, "", "txJoinDt");
                

                //txDocDt
                txtDocDate = oForm.Items.Item("txDocDt").Specific;
                itxtDocDate = oForm.Items.Item("txDocDt");
                oForm.DataSources.UserDataSources.Add("txDocDt", SAPbouiCOM.BoDataType.dt_DATE);
                txtDocDate.DataBind.SetBound(true, "", "txDocDt");
                

                //txDocNo
                txtDocNo = oForm.Items.Item("txDocNo").Specific;
                itxtDocNo = oForm.Items.Item("txDocNo");
                oForm.DataSources.UserDataSources.Add("txDocNo", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                txtDocNo.DataBind.SetBound(true, "", "txDocNo");
                

                //txBSn
                txtBasicSalaryNew = oForm.Items.Item("txBSn").Specific;
                itxtBasicSalaryNew = oForm.Items.Item("txBSn");
                oForm.DataSources.UserDataSources.Add("txBSn", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                txtBasicSalaryNew.DataBind.SetBound(true, "", "txBSn");
                

                //txJoinDtn
                txtJoiningDtNew = oForm.Items.Item("txJoinDtn").Specific;
                itxtJoiningDtNew = oForm.Items.Item("txJoinDtn");
                oForm.DataSources.UserDataSources.Add("txJoinDtn", SAPbouiCOM.BoDataType.dt_DATE);
                txtJoiningDtNew.DataBind.SetBound(true, "", "txJoinDtn");
                

                //cbLoc
                cbLocationNew = oForm.Items.Item("cbLoc").Specific;
                icbLocationNew = oForm.Items.Item("cbLoc");
                oForm.DataSources.UserDataSources.Add("cbLoc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbLocationNew.DataBind.SetBound(true, "", "cbLoc");

                //cbDept
                cbDepartmentNew = oForm.Items.Item("cbDept").Specific;
                icbDepartmentNew = oForm.Items.Item("cbDept");
                oForm.DataSources.UserDataSources.Add("cbDept", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbDepartmentNew.DataBind.SetBound(true, "", "cbDept");

                //cbDesig
                cbDesignationNew = oForm.Items.Item("cbDesig").Specific;
                icbDesignationNew = oForm.Items.Item("cbDesig");
                oForm.DataSources.UserDataSources.Add("cbDesig", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbDesignationNew.DataBind.SetBound(true, "", "cbDesig");

                //cbBranch
                cbBranchNew = oForm.Items.Item("cbBranch").Specific;
                icbBranchNew = oForm.Items.Item("cbBranch");
                oForm.DataSources.UserDataSources.Add("cbBranch", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbBranchNew.DataBind.SetBound(true, "", "cbBranch");

                btnMain = oForm.Items.Item("1").Specific;
                ibtnMain = oForm.Items.Item("1");
                btnCancel = oForm.Items.Item("2").Specific;
                ibtnCancel = oForm.Items.Item("2");
                btnReHire = oForm.Items.Item("5").Specific;
                ibtnReHire = oForm.Items.Item("5");
                btnEmpPick = oForm.Items.Item("3").Specific;
                ibtnEmpPick = oForm.Items.Item("3");
                btnMngPick = oForm.Items.Item("4").Specific;
                ibtnMngPick = oForm.Items.Item("4");

                FillDepartmentCombo(cbDepartmentNew);
                FillDesignationCombo(cbDesignationNew);
                FillBranchCombo(cbBranchNew);
                FillLocationsCombo(cbLocationNew);
                
                itxtEmpCode.Enabled = false;
                itxtEmpName.Enabled = false;
                itxtLocationOld.Enabled = false;
                itxtDepartmentOld.Enabled = false;
                itxtDesignationOld.Enabled = false;
                itxtBranchOld.Enabled = false;
                itxtManagerOld.Enabled = false;
                itxtBasicSalaryOld.Enabled = false;
                itxtGrossSalaryOld.Enabled = false;
                itxtTerminationDt.Enabled = false;
                itxtResignDt.Enabled = false;
                itxtJoiningDtOld.Enabled = false;
                //itxtDocDate.Enabled = false;
                //itxtDocNo.Enabled = false;
                icbBranchNew.DisplayDesc = true;
                icbDepartmentNew.DisplayDesc = true;
                icbDesignationNew.DisplayDesc = true;
                icbLocationNew.DisplayDesc = true;
                //itxtBasicSalaryNew.Enabled = false;
                //itxtJoiningDtNew.Enabled = false;

                GetAllObjects();
                ClearRecord();
                GetDataFilterData();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Initialize Form Error : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void OpenNewSearchForm()
        {
            try
            {
                Program.EmpID = "";
                string comName = "MstSearch";
                Program.sqlString = "empFSN";
                string strLang = "ln_English";
                try
                {
                    oApplication.Forms.Item("frm_" + comName).Select();
                }
                catch
                {
                    Type oFormType = Type.GetType("ACHR.Screen." + "frm_" + comName);
                    Screen.HRMSBaseForm objScr = ((Screen.HRMSBaseForm)oFormType.GetConstructor(System.Type.EmptyTypes).Invoke(null));
                    objScr.CreateForm(oApplication, "ACHR.XMLScreen." + strLang + ".xml_" + comName + ".xml", oCompany, "frm_" + comName);
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Function : OpenNewSearchForm Exception : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void OpenNewSearchFormManager()
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
                    Type oFormType = Type.GetType("ACHR.Screen." + "frm_" + comName);
                    Screen.HRMSBaseForm objScr = ((Screen.HRMSBaseForm)oFormType.GetConstructor(System.Type.EmptyTypes).Invoke(null));
                    objScr.CreateForm(oApplication, "ACHR.XMLScreen." + strLang + ".xml_" + comName + ".xml", oCompany, "frm_" + comName);
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Function : OpenNewSearchForm Exception : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void SetEmpValues()
        {
            try
            {
                if (!string.IsNullOrEmpty(Program.EmpID))
                {
                    txtEmpCode.Value = Program.EmpID;
                    SelectEmployee(Program.EmpID);
                }
            }
            catch (Exception ex)
            {
            }
        }
        
        private void SetManagerEmployee()
        {
            try
            {
                if (!string.IsNullOrEmpty(Program.EmpID))
                {
                    txtManagerNew.Value = Program.EmpID;
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void FillDepartmentCombo(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                var AllDepartment = from a in dbHrPayroll.MstDepartment orderby a.DeptName ascending select a;
                pCombo.ValidValues.Add("-1", "");
                foreach (var Dept in AllDepartment)
                {
                    pCombo.ValidValues.Add(Convert.ToString(Dept.ID), Convert.ToString(Dept.DeptName));
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void FillDesignationCombo(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                IEnumerable<MstDesignation> Designations = from a in dbHrPayroll.MstDesignation orderby a.Name ascending select a;
                pCombo.ValidValues.Add("-1", "");
                foreach (MstDesignation One in Designations)
                {
                    pCombo.ValidValues.Add(Convert.ToString(One.Id), Convert.ToString(One.Name));
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void FillBranchCombo(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                var AllBranches = from a in dbHrPayroll.MstBranches select a;
                pCombo.ValidValues.Add("-1", "");
                foreach (var Branch in AllBranches)
                {
                    pCombo.ValidValues.Add(Convert.ToString(Branch.Id), Convert.ToString(Branch.Description));
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void FillManagerCombo(SAPbouiCOM.ComboBox pCombo)
        {

            IEnumerable<MstEmployee> AllEmployee = from a in dbHrPayroll.MstEmployee select a;
            pCombo.ValidValues.Add("-1", "");
            foreach (MstEmployee Emp in AllEmployee)
            {
                pCombo.ValidValues.Add(Convert.ToString(Emp.ID), Convert.ToString(Emp.FirstName + " " + Emp.MiddleName + " " + Emp.LastName));
            }
            pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

        }

        private void FillLocationsCombo(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                IEnumerable<MstLocation> Locations = from a in dbHrPayroll.MstLocation orderby a.Name ascending select a;
                pCombo.ValidValues.Add("-1", "");
                foreach (MstLocation Location in Locations)
                {
                    pCombo.ValidValues.Add(Convert.ToString(Location.Id), Convert.ToString(Location.Name));
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void SelectEmployee(string pEmpID)
        {
            try
            {
                if (!string.IsNullOrEmpty(pEmpID))
                {
                    MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == pEmpID select a).FirstOrDefault();
                    if (oEmp != null)
                    {
                        txtDocNo.Value = ds.GetDocumentNumber(23, 23).ToString();
                        txtEmpCode.Value = oEmp.EmpID;
                        txtEmpName.Value = oEmp.FirstName + " " + oEmp.MiddleName + " " + oEmp.LastName;
                        txtDepartmentOld.Value = oEmp.DepartmentName;
                        txtDesignationOld.Value = oEmp.DesignationName;
                        txtLocationOld.Value = oEmp.LocationName;
                        txtBranchOld.Value = oEmp.BranchName;
                        string managerID = Convert.ToString(oEmp.Manager);
                        if (!string.IsNullOrEmpty(managerID))
                        {
                            MstEmployee oMng = (from a in dbHrPayroll.MstEmployee where a.ID.ToString() == managerID select a).FirstOrDefault();
                            if (oMng != null)
                            {
                                txtManagerOld.Value = oMng.FirstName + " " + oMng.MiddleName + " " + oMng.LastName;
                            }
                        }
                        txtBasicSalaryOld.Value = Convert.ToDecimal(oEmp.BasicSalary).ToString("######00.00");
                        txtGrossSalaryOld.Value = Convert.ToDouble(ds.getEmpGross(oEmp)).ToString("######00.00");
                        txtTerminationDt.Value = Convert.ToDateTime(oEmp.TerminationDate).ToString("yyyyMMdd");
                        txtResignDt.Value = Convert.ToDateTime(oEmp.ResignDate).ToString("yyyyMMdd");
                        txtJoiningDtOld.Value = Convert.ToDateTime(oEmp.JoiningDate).ToString("yyyyMMdd");
                        if (oEmp.DepartmentID != null)
                        {
                            cbDepartmentNew.Select( oEmp.DepartmentID.ToString(), SAPbouiCOM.BoSearchKey.psk_ByValue);
                        }
                        else
                        {
                            cbDepartmentNew.Select("-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                        }
                        if (oEmp.DesignationID != null)
                        {
                            cbDesignationNew.Select(oEmp.DesignationID.ToString(), SAPbouiCOM.BoSearchKey.psk_ByValue);
                        }
                        else
                        {
                            cbDesignationNew.Select("-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                        }
                        if (oEmp.BranchID != null)
                        {
                            cbBranchNew.Select(oEmp.BranchID.ToString(), SAPbouiCOM.BoSearchKey.psk_ByValue);
                        }
                        else
                        {
                            cbBranchNew.Select("-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                        }
                        if (oEmp.Location != null)
                        {
                            cbLocationNew.Select(oEmp.Location.ToString(), SAPbouiCOM.BoSearchKey.psk_ByValue);
                        }
                        else
                        {
                            cbLocationNew.Select("-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                        }
                        if (oEmp.Manager != null)
                        {
                            string managerIDNew = Convert.ToString(oEmp.Manager);
                            if (!string.IsNullOrEmpty(managerIDNew))
                            {
                                MstEmployee oMngNew = (from a in dbHrPayroll.MstEmployee where a.ID.ToString() == managerID select a).FirstOrDefault();
                                if (oMngNew != null)
                                {
                                    txtManagerOld.Value = oMngNew.EmpID;
                                }
                            }
                        }
                        else
                        {
                            
                        }
                    }
                }
            }
            catch(Exception ex)
            {
            }
        }

        private Boolean AddRecord()
        {
            try
            {
                string empcode = txtEmpCode.Value.Trim();
                if (!string.IsNullOrEmpty(empcode))
                {
                    MstEmployee oEmpDoc = (from a in dbHrPayroll.MstEmployee where a.EmpID == empcode select a).FirstOrDefault();
                    if (oEmpDoc != null)
                    {
                        TrnsEmployeeReHire oDoc = new TrnsEmployeeReHire();
                        oDoc.DocNo = Convert.ToInt32(txtDocNo.Value.Trim());
                        oDoc.MstEmployee = oEmpDoc;
                        oDoc.EmployeeName = oEmpDoc.FirstName + " " + oEmpDoc.MiddleName + " " + oEmpDoc.LastName;
                        if (txtDocDate.Value != "")
                        {
                            oDoc.DocDate = DateTime.ParseExact(txtDocDate.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        }
                        else
                        {
                            oDoc.DocDate = null;
                        }
                        if (txtJoiningDtOld.Value != "")
                        {
                            oDoc.JoiningDtOld = DateTime.ParseExact(txtJoiningDtOld.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        }
                        else
                        {
                            oDoc.JoiningDtOld = null;
                        }
                        if (txtResignDt.Value != "")
                        {
                            oDoc.ResignationDtOld = DateTime.ParseExact(txtResignDt.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        }
                        else
                        {
                            oDoc.ResignationDtOld = null;
                        }
                        if (txtTerminationDt.Value != "")
                        {
                            oDoc.TerminationDtOld = DateTime.ParseExact(txtTerminationDt.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        }
                        else
                        {
                            oDoc.TerminationDtOld = null;
                        }
                        if (txtJoiningDtNew.Value != "")
                        {
                            oDoc.JoiningDtNew = DateTime.ParseExact(txtJoiningDtNew.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        }
                        else
                        {
                            oDoc.JoiningDtNew = null;
                        }
                        if (string.IsNullOrEmpty(txtBasicSalaryOld.Value))
                        {
                            oDoc.BSOld = 0;
                        }
                        else
                        {
                            oDoc.BSOld = Convert.ToDecimal(txtBasicSalaryOld.Value.Trim());
                        }
                        if (string.IsNullOrEmpty(txtBasicSalaryNew.Value))
                        {
                            oDoc.BSNew = 0;
                        }
                        else
                        {
                            oDoc.BSNew = Convert.ToDecimal(txtBasicSalaryNew.Value.Trim());
                        }
                        if (string.IsNullOrEmpty(txtGrossSalaryOld.Value))
                        {
                            oDoc.GSOld = 0;
                        }
                        else
                        {
                            oDoc.GSOld = Convert.ToDecimal(txtGrossSalaryOld.Value.Trim());
                        }
                        if (string.IsNullOrEmpty(txtManagerOld.Value))
                        {
                            oDoc.ManagerIDOld = null;
                            oDoc.ManagerNameOld = null;
                        }
                        else
                        {
                            string managerEmpCode = txtManagerOld.Value.Trim();
                            MstEmployee oMngEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == managerEmpCode select a).FirstOrDefault();
                            if (oMngEmp != null)
                            {
                                oDoc.ManagerIDOld = oMngEmp.ID;
                                oDoc.ManagerNameOld = oMngEmp.FirstName + " " + oMngEmp.MiddleName + " " + oMngEmp.LastName;
                            }
                        }
                        if (oEmpDoc.DepartmentID != null)
                        {
                            oDoc.DepartmentOld = oEmpDoc.DepartmentID;
                            oDoc.DepartNameOld = oEmpDoc.DepartmentName;
                        }
                        if (oEmpDoc.DesignationID != null)
                        {
                            oDoc.DesignationOld = oEmpDoc.DesignationID;
                            oDoc.DesigNameOld = oEmpDoc.DesignationName;
                        }
                        if (oEmpDoc.Location != null)
                        {
                            oDoc.LocationOld = oEmpDoc.Location;
                            oDoc.LocNameOld = oEmpDoc.LocationName;
                        }
                        if (oEmpDoc.BranchID != null)
                        {
                            oDoc.BranchOld = oEmpDoc.BranchID;
                            oDoc.BranchNameOld = oEmpDoc.BranchName;
                        }
                        if (cbDepartmentNew.Value.Trim() != "-1")
                        {
                            Int32 DeptID = Convert.ToInt32(cbDepartmentNew.Value.Trim());
                            MstDepartment oDept = (from a in dbHrPayroll.MstDepartment where a.ID == DeptID select a).FirstOrDefault();
                            oDoc.DepartmentIDNew = oDept.ID;
                            oDoc.DepartmentNameNew = oDept.DeptName;
                        }
                        else
                        {
                            oDoc.DepartmentIDNew = null;
                            oDoc.DepartmentNameNew = null;
                        }
                        if (cbDesignationNew.Value.Trim() != "-1")
                        {
                            Int32 DesigID = Convert.ToInt32(cbDesignationNew.Value.Trim());
                            MstDesignation oDesig = (from a in dbHrPayroll.MstDesignation where a.Id == DesigID select a).FirstOrDefault();
                            oDoc.DesignationIDNew = oDesig.Id;
                            oDoc.DesignationNameNew = oDesig.Name;
                        }
                        else
                        {
                            oDoc.DesignationIDNew = null;
                            oDoc.DesignationNameNew = "";
                        }
                        if (cbBranchNew.Value.Trim() != "-1")
                        {
                            Int32 BranchID = Convert.ToInt32(cbBranchNew.Value.Trim());
                            MstBranches oBranch = (from a in dbHrPayroll.MstBranches where a.Id == BranchID select a).FirstOrDefault();
                            oDoc.BranchIDNew = oBranch.Id;
                            oDoc.BranchNameNew = oBranch.Name;
                        }
                        else
                        {
                            oDoc.BranchIDNew = null;
                            oDoc.BranchNameNew = null;
                        }
                        if (cbLocationNew.Value.Trim() != "-1")
                        {
                            Int32 LocId = Convert.ToInt32(cbLocationNew.Value.Trim());
                            MstLocation Location = (from a in dbHrPayroll.MstLocation where a.Id == LocId select a).FirstOrDefault();
                            oDoc.LocationIDNew = Location.Id;
                            oDoc.LocationNameNew = Location.Name;
                        }
                        else
                        {
                            oDoc.LocationIDNew = null;
                            oDoc.LocationNameNew = "";
                        }
                        if (string.IsNullOrEmpty(txtManagerNew.Value))
                        {
                            oDoc.ManagerIDNew = null;
                            oDoc.ManagerNameNew = null;
                        }
                        else
                        {
                            string managerEmpCode = txtManagerNew.Value.Trim();
                            MstEmployee oMngEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == managerEmpCode select a).FirstOrDefault();
                            if (oMngEmp != null)
                            {
                                oDoc.ManagerIDNew = oMngEmp.ID;
                                oDoc.ManagerNameNew = oMngEmp.FirstName + " " + oMngEmp.MiddleName + " " + oMngEmp.LastName;
                            }
                        }
                        Int32 TermCount = 0;
                        TermCount = Convert.ToInt32((from a in dbHrPayroll.TrnsEmployeeReHire where a.MstEmployee.EmpID == txtEmpCode.Value.Trim() select a).Count());
                        oDoc.TermCount = TermCount + 1;
                        oDoc.CreatedBy = oCompany.UserName;
                        oDoc.UpdatedBy = oCompany.UserName;
                        oDoc.CreateDt = DateTime.Now;
                        oDoc.UpdateDt = DateTime.Now;
                        dbHrPayroll.TrnsEmployeeReHire.InsertOnSubmit(oDoc);
                        dbHrPayroll.SubmitChanges();
                        oApplication.StatusBar.SetText("Document Added Successfully.", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                        return true;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Error Add : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                return false;
            }
        }

        private Boolean UpdateRecord()
        {
            try
            {
                string empcode = txtEmpCode.Value.Trim();
                if (!string.IsNullOrEmpty(empcode))
                {
                    MstEmployee oEmpDoc = (from a in dbHrPayroll.MstEmployee where a.EmpID == empcode select a).FirstOrDefault();
                    if (oEmpDoc != null)
                    {
                        string docno = txtDocNo.Value.Trim();
                        TrnsEmployeeReHire oDoc = (from a in dbHrPayroll.TrnsEmployeeReHire where a.MstEmployee.EmpID == empcode && a.DocNo.ToString() == docno select a).FirstOrDefault();
                        if (oDoc != null)
                        {
                            oDoc.DocNo = Convert.ToInt32(txtDocNo.Value.Trim());
                            oDoc.MstEmployee = oEmpDoc;
                            oDoc.EmployeeName = oEmpDoc.FirstName + " " + oEmpDoc.MiddleName + " " + oEmpDoc.LastName;
                            if (txtDocDate.Value != "")
                            {
                                oDoc.DocDate = DateTime.ParseExact(txtDocDate.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                            }
                            else
                            {
                                oDoc.DocDate = null;
                            }
                            if (txtJoiningDtOld.Value != "")
                            {
                                oDoc.JoiningDtOld = DateTime.ParseExact(txtJoiningDtOld.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                            }
                            else
                            {
                                oDoc.JoiningDtOld = null;
                            }
                            if (txtResignDt.Value != "")
                            {
                                oDoc.ResignationDtOld = DateTime.ParseExact(txtResignDt.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                            }
                            else
                            {
                                oDoc.ResignationDtOld = null;
                            }
                            if (txtTerminationDt.Value != "")
                            {
                                oDoc.TerminationDtOld = DateTime.ParseExact(txtTerminationDt.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                            }
                            else
                            {
                                oDoc.TerminationDtOld = null;
                            }
                            if (txtJoiningDtNew.Value != "")
                            {
                                oDoc.JoiningDtNew = DateTime.ParseExact(txtJoiningDtNew.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                            }
                            else
                            {
                                oDoc.JoiningDtNew = null;
                            }
                            if (string.IsNullOrEmpty(txtBasicSalaryOld.Value))
                            {
                                oDoc.BSOld = 0;
                            }
                            else
                            {
                                oDoc.BSOld = Convert.ToDecimal(txtBasicSalaryOld.Value.Trim());
                            }
                            if (string.IsNullOrEmpty(txtBasicSalaryNew.Value))
                            {
                                oDoc.BSNew = 0;
                            }
                            else
                            {
                                oDoc.BSNew = Convert.ToDecimal(txtBasicSalaryNew.Value.Trim());
                            }
                            if (string.IsNullOrEmpty(txtGrossSalaryOld.Value))
                            {
                                oDoc.GSOld = 0;
                            }
                            else
                            {
                                oDoc.GSOld = Convert.ToDecimal(txtGrossSalaryOld.Value.Trim());
                            }
                            if (string.IsNullOrEmpty(txtManagerOld.Value))
                            {
                                oDoc.ManagerIDOld = null;
                                oDoc.ManagerNameOld = null;
                            }
                            else
                            {
                                string managerEmpCode = txtManagerOld.Value.Trim();
                                MstEmployee oMngEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == managerEmpCode select a).FirstOrDefault();
                                if (oMngEmp != null)
                                {
                                    oDoc.ManagerIDOld = oMngEmp.ID;
                                    oDoc.ManagerNameOld = oMngEmp.FirstName + " " + oMngEmp.MiddleName + " " + oMngEmp.LastName;
                                }
                            }
                            if (oEmpDoc.DepartmentID != null)
                            {
                                oDoc.DepartmentOld = oEmpDoc.DepartmentID;
                                oDoc.DepartNameOld = oEmpDoc.DepartmentName;
                            }
                            if (oEmpDoc.DesignationID != null)
                            {
                                oDoc.DesignationOld = oEmpDoc.DesignationID;
                                oDoc.DesigNameOld = oEmpDoc.DesignationName;
                            }
                            if (oEmpDoc.Location != null)
                            {
                                oDoc.LocationOld = oEmpDoc.Location;
                                oDoc.LocNameOld = oEmpDoc.LocationName;
                            }
                            if (oEmpDoc.BranchID != null)
                            {
                                oDoc.BranchOld = oEmpDoc.BranchID;
                                oDoc.BranchNameOld = oEmpDoc.BranchName;
                            }
                            if (cbDepartmentNew.Value.Trim() != "-1")
                            {
                                Int32 DeptID = Convert.ToInt32(cbDepartmentNew.Value.Trim());
                                MstDepartment oDept = (from a in dbHrPayroll.MstDepartment where a.ID == DeptID select a).FirstOrDefault();
                                oDoc.DepartmentIDNew = oDept.ID;
                                oDoc.DepartmentNameNew = oDept.DeptName;
                            }
                            else
                            {
                                oDoc.DepartmentIDNew = null;
                                oDoc.DepartmentNameNew = null;
                            }
                            if (cbDesignationNew.Value.Trim() != "-1")
                            {
                                Int32 DesigID = Convert.ToInt32(cbDesignationNew.Value.Trim());
                                MstDesignation oDesig = (from a in dbHrPayroll.MstDesignation where a.Id == DesigID select a).FirstOrDefault();
                                oDoc.DesignationIDNew = oDesig.Id;
                                oDoc.DesignationNameNew = oDesig.Name;
                            }
                            else
                            {
                                oDoc.DesignationIDNew = null;
                                oDoc.DesignationNameNew = "";
                            }
                            if (cbBranchNew.Value.Trim() != "-1")
                            {
                                Int32 BranchID = Convert.ToInt32(cbBranchNew.Value.Trim());
                                MstBranches oBranch = (from a in dbHrPayroll.MstBranches where a.Id == BranchID select a).FirstOrDefault();
                                oDoc.BranchIDNew = oBranch.Id;
                                oDoc.BranchNameNew = oBranch.Name;
                            }
                            else
                            {
                                oDoc.BranchIDNew = null;
                                oDoc.BranchNameNew = null;
                            }
                            if (cbLocationNew.Value.Trim() != "-1")
                            {
                                Int32 LocId = Convert.ToInt32(cbLocationNew.Value.Trim());
                                MstLocation Location = (from a in dbHrPayroll.MstLocation where a.Id == LocId select a).FirstOrDefault();
                                oDoc.LocationIDNew = Location.Id;
                                oDoc.LocationNameNew = Location.Name;
                            }
                            else
                            {
                                oDoc.LocationIDNew = null;
                                oDoc.LocationNameNew = "";
                            }
                            if (string.IsNullOrEmpty(txtManagerNew.Value))
                            {
                                oDoc.ManagerIDNew = null;
                                oDoc.ManagerNameNew = null;
                            }
                            else
                            {
                                string managerEmpCode = txtManagerNew.Value.Trim();
                                MstEmployee oMngEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == managerEmpCode select a).FirstOrDefault();
                                if (oMngEmp != null)
                                {
                                    oDoc.ManagerIDNew = oMngEmp.ID;
                                    oDoc.ManagerNameNew = oMngEmp.FirstName + " " + oMngEmp.MiddleName + " " + oMngEmp.LastName;
                                }
                            }
                            oDoc.UpdatedBy = oCompany.UserName;
                            oDoc.UpdateDt = DateTime.Now;
                            dbHrPayroll.SubmitChanges();
                            oApplication.StatusBar.SetText("Document Updated Successfully.", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                            return true;
                        }
                        else
                        {
                            oApplication.StatusBar.SetText("Document Not Found.", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                    }
                    else
                    {
                        oApplication.StatusBar.SetText("Employee Not Found.", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Error UpdateRecord : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                return false;
            }
        }

        private void ClearRecord()
        {
            try
            {
                Program.EmpID = "";
                OpenDocID = 0;
                txtEmpCode.Value = "";
                txtEmpName.Value = "";
                txtDepartmentOld.Value = "";
                txtDesignationOld.Value = "";
                txtLocationOld.Value = "";
                txtBranchOld.Value = "";
                txtManagerOld.Value = "";
                txtManagerNew.Value = "";
                txtDocNo.Value = Convert.ToString(ds.GetDocumentNumber(23, 23));
                txtDocDate.Value = "";
                txtBasicSalaryOld.Value = "";
                txtGrossSalaryOld.Value = "";
                txtJoiningDtOld.Value = "";
                txtJoiningDtNew.Value = "";
                txtResignDt.Value = "";
                txtTerminationDt.Value = "";
                txtBasicSalaryNew.Value = "";
                cbDepartmentNew.Select("-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                cbDesignationNew.Select("-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                cbLocationNew.Select("-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                cbBranchNew.Select("-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            }
            catch
            {
            }
        }

        private void FillRecord(TrnsEmployeeReHire oDoc)
        {
            try
            {
                if (oDoc != null)
                {
                    OpenDocID = Convert.ToInt32(oDoc.InternalID);
                    txtDocNo.Value = Convert.ToString(oDoc.DocNo);
                    txtEmpCode.Value = Convert.ToString(oDoc.MstEmployee.EmpID);
                    txtEmpName.Value = Convert.ToString(oDoc.EmployeeName);
                    txtLocationOld.Value = Convert.ToString(oDoc.LocNameOld);
                    txtDepartmentOld.Value = Convert.ToString(oDoc.DepartNameOld);
                    txtDesignationOld.Value = Convert.ToString(oDoc.DesigNameOld);
                    txtBranchOld.Value = Convert.ToString(oDoc.BranchNameOld);
                    txtBasicSalaryOld.Value = Convert.ToDecimal(oDoc.BSOld).ToString("######00.00");
                    txtGrossSalaryOld.Value = Convert.ToDecimal(oDoc.GSOld).ToString("######00.00");
                    txtBasicSalaryNew.Value = Convert.ToDecimal(oDoc.BSNew).ToString("######00.00");
                    if (oDoc.DocDate != null)
                    {
                        if (oDoc.DocDate > DateTime.MinValue)
                        {
                            txtDocDate.Value = Convert.ToDateTime(oDoc.DocDate).ToString("yyyyMMdd");
                        }
                        else
                        {
                            txtDocDate.Value = "";
                        }
                    }
                    else
                    {
                        txtDocDate.Value = "";
                    }
                    if (oDoc.JoiningDtOld != null)
                    {
                        if (oDoc.JoiningDtOld > DateTime.MinValue)
                        {
                            txtJoiningDtOld.Value = Convert.ToDateTime(oDoc.JoiningDtOld).ToString("yyyyMMdd");
                        }
                        else
                        {
                            txtJoiningDtOld.Value = "";
                        }
                    }
                    else
                    {
                        txtJoiningDtOld.Value = "";
                    }
                    if (oDoc.JoiningDtNew != null)
                    {
                        if (oDoc.JoiningDtNew > DateTime.MinValue)
                        {
                            txtJoiningDtNew.Value = Convert.ToDateTime(oDoc.JoiningDtNew).ToString("yyyyMMdd");
                        }
                        else
                        {
                            txtJoiningDtNew.Value = "";
                        }
                    }
                    else
                    {
                        txtJoiningDtNew.Value = "";
                    }
                    if (oDoc.ResignationDtOld != null)
                    {
                        if (oDoc.ResignationDtOld > DateTime.MinValue)
                        {
                            txtResignDt.Value = Convert.ToDateTime(oDoc.ResignationDtOld).ToString("yyyyMMdd");
                        }
                        else
                        {
                            txtResignDt.Value = "";
                        }
                    }
                    else
                    {
                        txtResignDt.Value = "";
                    }
                    if (oDoc.TerminationDtOld != null)
                    {
                        if (oDoc.TerminationDtOld > DateTime.MinValue)
                        {
                            txtTerminationDt.Value = Convert.ToDateTime(oDoc.TerminationDtOld).ToString("yyyyMMdd");
                        }
                        else
                        {
                            txtTerminationDt.Value = "";
                        }
                    }
                    else
                    {
                        txtTerminationDt.Value = "";
                    }
                    if (oDoc.ManagerIDOld != null)
                    {
                        if (oDoc.ManagerIDOld != -1)
                        {
                            MstEmployee mngEmp = (from a in dbHrPayroll.MstEmployee where a.ID == oDoc.ManagerIDOld select a).FirstOrDefault();
                            txtManagerOld.Value = mngEmp.EmpID;
                        }
                    }
                    else
                    {
                        txtManagerOld.Value = "";
                    }
                    if (oDoc.ManagerIDNew != null)
                    {
                        if (oDoc.ManagerIDNew != -1)
                        {
                            MstEmployee mngEmp = (from a in dbHrPayroll.MstEmployee where a.ID == oDoc.ManagerIDNew select a).FirstOrDefault();
                            txtManagerNew.Value = mngEmp.EmpID;
                        }
                    }
                    else
                    {
                        txtManagerNew.Value = "";
                    }
                    cbLocationNew.Select(oDoc.LocationIDNew != null ? oDoc.LocationIDNew.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                    cbDepartmentNew.Select(oDoc.DepartmentIDNew != null ? oDoc.DepartmentIDNew.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                    cbDesignationNew.Select(oDoc.DesignationIDNew != null ? oDoc.DesignationIDNew.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                    cbBranchNew.Select(oDoc.BranchIDNew != null ? oDoc.BranchIDNew.ToString() : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                    if (!Convert.ToBoolean(oDoc.FlgReHire))
                    {
                        ibtnReHire.Enabled = true;
                    }
                    else
                    {
                        ibtnReHire.Enabled = false;
                    }
                    ibtnEmpPick.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("FillRecord : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void ReHireEmployeeMaster()
        {
            try
            {
                TrnsEmployeeReHire oDoc = (from a in dbHrPayroll.TrnsEmployeeReHire where a.InternalID == OpenDocID select a).FirstOrDefault();
                if (oDoc != null)
                {
                    MstEmployee oEmpReHire = (from a in dbHrPayroll.MstEmployee where a.ID.ToString() == oDoc.MstEmployee.ID.ToString() select a).FirstOrDefault();
                    if (oEmpReHire != null)
                    {
                        oEmpReHire.ResignDate = null;
                        oEmpReHire.TerminationDate = null;
                        oEmpReHire.FlgActive = true;
                        if (oEmpReHire.TermCount != null)
                        {
                            int termCount = Convert.ToInt32(oEmpReHire.TermCount);
                            oEmpReHire.TermCount = termCount + 1;
                        }
                        else
                        {
                            oEmpReHire.TermCount = 1;
                        }
                        oEmpReHire.BasicSalary = Convert.ToDecimal(oDoc.BSNew);
                        oEmpReHire.JoiningDate = oDoc.JoiningDtNew;
                        oEmpReHire.PaymentMode = "CASH";
                        dbHrPayroll.SubmitChanges();
                        oDoc.FlgReHire = true;
                        dbHrPayroll.SubmitChanges();
                        oApplication.StatusBar.SetText("Employee Successfully ReHired.", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                        ibtnReHire.Enabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("ReHireEmployeeMaster : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void GetAllObjects()
        {
            try
            {
                CodeIndex.Clear();
                oDocuments = (from a in dbHrPayroll.TrnsEmployeeReHire select a).ToList();
                Int32 i = 0;
                foreach (TrnsEmployeeReHire oDoc in oDocuments)
                {
                    CodeIndex.Add(oDoc.InternalID.ToString(), i);
                    i++;
                }
                totalRecord = i;
            }
            catch (Exception ex)
            {
            }
        }

        private Boolean ValidateAdd()
        {
            try
            {
                if (string.IsNullOrEmpty(txtJoiningDtNew.Value))
                {
                    oApplication.StatusBar.SetText("New Joining Date is mandatory.", SAPbouiCOM.BoMessageTime.bmt_Long, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                if (string.IsNullOrEmpty(txtBasicSalaryNew.Value))
                {
                    oApplication.StatusBar.SetText("New Basic Salary is mandatory.", SAPbouiCOM.BoMessageTime.bmt_Long, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                if (string.IsNullOrEmpty(txtManagerNew.Value) && !string.IsNullOrEmpty(txtManagerOld.Value))
                {
                    oApplication.StatusBar.SetText("New Manager is mandatory.", SAPbouiCOM.BoMessageTime.bmt_Long, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                if (string.IsNullOrEmpty(txtDocDate.Value))
                {
                    oApplication.StatusBar.SetText("Document Date is mandatory.", SAPbouiCOM.BoMessageTime.bmt_Long, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private Boolean ValidateUpdate()
        {
            try
            {
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void DoAction()
        {
            try
            {
                if (btnMain.Caption == "Add")
                {
                    if (ValidateAdd())
                    {
                        if (AddRecord())
                        {
                            ClearRecord();
                            GetAllObjects();
                        }
                    }
                }
                if (btnMain.Caption == "Update")
                {
                    if (ValidateUpdate())
                    {
                        if (UpdateRecord())
                        {
                            ClearRecord();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
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
