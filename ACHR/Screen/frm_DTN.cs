using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using DIHRMS.Custom;
using System.Data;
using System.Data.SqlClient;
using SAPbobsCOM;
using System.Collections;
using System.IO;
using UFFU;

namespace ACHR.Screen
{
    class frm_DTN : HRMSBaseForm
    {
        #region "Variables"

        SAPbouiCOM.EditText txtDB, txtFileName, txAccrDate, txHrmsId;
        SAPbouiCOM.ComboBox cmbImportedType;
        SAPbouiCOM.Matrix grdDisplay;
        SAPbouiCOM.Button btnPick, btnPullData, btnProgram, btnImport;
        SAPbouiCOM.OptionBtn optNew;
        SAPbouiCOM.Item ItxDb, ItxFilenam, ItxAccrDate, ItxHrmsId;
        SAPbouiCOM.Item IcbImpObj;
        SAPbouiCOM.Item ImtObj;
        SAPbouiCOM.Item IbtPick, IbtPull, IbtPrg, IbtImport;
        SAPbouiCOM.DataTable dtHead, dtMat;
        Boolean flgUpdateCheck = true;
        #endregion

        #region "B1 Events"

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            try
            {
                oForm.Freeze(true);
                oForm.EnableMenu("1290", false);  // First Record
                oForm.EnableMenu("1291", false);  // Last record 
                InitiallizeForm();
                fillObj();
                oForm.Freeze(false);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_DTN Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }

        }

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "btPull":
                    PullDataClick();
                    break;
                case "btImport":
                    ImportDataInHRMS();
                    break;
                case "btPick":
                    getFileName();
                    break;

            }
        }

        #endregion

        #region "Functions"

        private void InitiallizeForm()
        {
            try
            {
                oForm.Freeze(true);

                cmbImportedType = oForm.Items.Item("cbImpObj").Specific;
                dtHead = oForm.DataSources.DataTables.Item("dtHead");
                dtMat = oForm.DataSources.DataTables.Item("dtMat");
                grdDisplay = oForm.Items.Item("mtObj").Specific;
                ImtObj = oForm.Items.Item("mtObj");
                dtHead.Rows.Add(1);
                btnImport = oForm.Items.Item("btImport").Specific;
                IbtImport = oForm.Items.Item("btImport");
                txtFileName = oForm.Items.Item("txFilenam").Specific;
                ItxFilenam = oForm.Items.Item("txFilenam");

                oForm.Freeze(false);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("InitializeFrom Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }

        }

        private void fillObj()
        {
            cmbImportedType.ValidValues.Add("-1", "[Select One]");
            cmbImportedType.ValidValues.Add("1", "Employee Import And Update");
            cmbImportedType.ValidValues.Add("2", "Per Piece Rate Employee Wise");
            cmbImportedType.ValidValues.Add("3", "Shift Import");
            cmbImportedType.ValidValues.Add("4", "Leave Import");
            cmbImportedType.ValidValues.Add("5", "Cancel Leave");
            cmbImportedType.ValidValues.Add("6", "Employee Relatives");
            cmbImportedType.ValidValues.Add("7", "Employee Experiance");
            cmbImportedType.ValidValues.Add("8", "Employee Certification");
            cmbImportedType.ValidValues.Add("9", "Employee Education");
            //cbImpObj.ValidValues.Add("3", "Branches");
            //cbImpObj.ValidValues.Add("4", "Designations");
            //cbImpObj.ValidValues.Add("5", "Positions");
            //cbImpObj.ValidValues.Add("6", "Department");
            cmbImportedType.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
        }

        private void getFileName()
        {
            try
            {
                string fileName = Program.objHrmsUI.FindFile();
                txtFileName.Value = fileName;
            }
            catch (Exception ex)
            {
            }
        }

        private void PullDataClick()
        {
            switch (cmbImportedType.Value.Trim())
            {
                case "1":
                case "2":
                case "3":
                case "4":
                case "5":
                case "6":
                case "7":
                case "8":
                    PullDataFromFile();
                    break;
                default:
                    oApplication.StatusBar.SetText("Selected Value didn't implemented yet.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    break;
            }

        }

        private void PullDataFromFile()
        {
            try
            {
                string fileName = txtFileName.Value.Trim();
                if (fileName == "") return;
                DataTable dttemp = new DataTable();
                FillDatatableFromTemplate(dttemp);
                PopulateGrid(dttemp);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("PullDataFromFile : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillDatatableFromTemplate(DataTable pdt)
        {
            try
            {
                string fileName = txtFileName.Value.Trim();
                if (fileName == "")
                {
                    oApplication.SetStatusBarMessage("Select a template file");
                }

                using (StreamReader File = new StreamReader(fileName))
                {
                    string OneLine = "";
                    string[] OneLineParsed;
                    string strTemplateName = File.ReadLine();
                    if (strTemplateName == null || !strTemplateName.Contains("HRMS Template"))
                    {
                        oApplication.SetStatusBarMessage("Incorrect Template File");
                        return;
                    }
                    OneLine = File.ReadLine();
                    if (OneLine == null)
                    {
                        oApplication.SetStatusBarMessage("Incorrect Template File");
                        return;
                    }
                    //OneLineParsed = OneLine.Split('\t');
                    OneLineParsed = OneLine.Split(',');
                    int cCount = 0;
                    foreach (string colName in OneLineParsed)
                    {
                        pdt.Columns.Add(colName);
                        cCount++;
                    }
                    int i = 1;
                    while ("a" == "a")
                    {
                        OneLine = File.ReadLine();
                        if (OneLine == null) break;
                        //OneLineParsed = OneLine.Split('\t');
                        OneLineParsed = OneLine.Split(',');
                        int cvCount = 0;
                        for (int cc = 0; cc < OneLineParsed.Count(); cc++)
                        {
                            OneLineParsed[cc] = OneLineParsed[cc].Trim();
                            cvCount++;
                        }

                        pdt.Rows.Add(OneLineParsed);
                    }
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void PopulateGrid(DataTable pdt)
        {

            dtMat.Clear();
            SAPbouiCOM.Item oItem;
            SAPbouiCOM.Columns oColumns;
            SAPbouiCOM.DataColumns dtCols;
            dtCols = dtMat.Columns;
            //IbtPrg.Width = 0;

            //int maxWidt = ImtObj.Width;
            int rcnt = pdt.Rows.Count;
            SAPbouiCOM.ProgressBar prg = oApplication.StatusBar.CreateProgressBar("Loading Data", 17, true);
            try
            {

                //int factor = Convert.ToInt16(maxWidt / pdt.Rows.Count);

                SAPbouiCOM.Column oColumn;
                SAPbouiCOM.DataColumn dtCol;
                oColumns = grdDisplay.Columns;
                dtMat.Rows.Clear();
                int i = 0;
                int j = 0;
                try
                {
                    grdDisplay.LoadFromDataSource();
                    int r = oColumns.Count - 1;
                    for (int m = 0; m < r; m++)
                    {
                        oColumns.Remove("v_" + m.ToString());
                    }
                }
                catch { }
                oForm.Freeze(true);
                foreach (System.Data.DataColumn cl in pdt.Columns)
                {
                    oColumn = oColumns.Add("v_" + i.ToString(), SAPbouiCOM.BoFormItemTypes.it_EDIT);
                    oColumn.TitleObject.Caption = cl.Caption;
                    oColumn.Width = 100;
                    oColumn.Editable = false;
                    dtCol = dtCols.Add(cl.ColumnName, SAPbouiCOM.BoFieldsType.ft_AlphaNumeric);
                    oColumn.DataBind.Bind("dtMat", cl.ColumnName);

                    i++;

                }
                dtMat.Rows.Clear();
                oForm.Freeze(false);
                i = 0;
                j = 0;

                foreach (DataRow dr in pdt.Rows)
                {

                    dtMat.Rows.Add(1);
                    j = 0;
                    foreach (System.Data.DataColumn col in pdt.Columns)
                    {
                        dtMat.SetValue(col.ColumnName, i, dr[j].ToString());
                        j++;
                    }
                    i++;
                    prg.Value = i;

                }

                oForm.Freeze(true);
                grdDisplay.LoadFromDataSource();
                oForm.Freeze(false);
                prg.Stop();
            }
            catch (Exception ex)
            {
                prg.Stop();
            }
        }

        private void ImportDataInHRMS()
        {
            switch (cmbImportedType.Value.Trim())
            {
                case "1":
                    postEmployee();
                    break;
                case "2":
                    PostEmployeePerPieceRate();
                    break;
                case "3":
                    PostShiftData();
                    break;
                case "4":
                    PostLeavesData();
                    break;
                case "5":
                    DeletePostLeavesData();
                    break;
                case "6":
                    PostRelativeData();
                    break;
                case "7":
                    PostExperianceData();
                    break;
                case "8":
                    PostCertificationData();
                    break;
                case "9":
                    PostEducationData();
                    break;
            }

        }

        private void postEmployee()
        {
            Int32 ErrorAtLine = 1;
            String EmpID = "";
            decimal decOldBasicSalary = 0, decOldGrosssalary = 0;
            Int32 intPayrollID = 0;
            //Comment New Work
            try
            {
                if (dtMat.Rows.Count > 0)
                {
                    for (int i = 0; i < dtMat.Rows.Count; i++)
                    {
                        string strEmpCode = dtMat.GetValue("EmpID", i);
                        strEmpCode = strEmpCode.Trim();
                        MstEmployee employee;
                        MstUsers empuser;
                        TrnsSalaryProcessRegister ProcessedSalary = null;
                        string usercode = dtMat.GetValue("UserCode", i);
                        int cnt = (from p in dbHrPayroll.MstEmployee where p.EmpID == strEmpCode.Trim() select p).Count();
                        if (cnt > 0)
                        {
                            employee = (from p in dbHrPayroll.MstEmployee where p.EmpID == strEmpCode.Trim() select p).FirstOrDefault();
                            try
                            {
                                empuser = employee.MstUsers.ElementAt(0);
                            }
                            catch
                            {
                                empuser = new MstUsers();
                                employee.MstUsers.Add(empuser);
                                employee.FlgUser = true;
                            }
                            if (dtMat.GetValue("UserCode", i) != "")
                            {
                                empuser.UserCode = dtMat.GetValue("UserCode", i);
                                empuser.UserID = dtMat.GetValue("UserCode", i);
                                empuser.PassCode = dtMat.GetValue("Password", i);
                                empuser.FlgWebUser = true;
                            }
                        }
                        else
                        {
                            employee = new MstEmployee();
                            empuser = new MstUsers();

                            employee.FlgUser = true;
                            employee.IntSboTransfered = false;
                            employee.IntSboPublished = true;
                            if (usercode.Trim() == "")
                            {
                                empuser.UserCode = strEmpCode.Trim();
                                empuser.UserID = strEmpCode.Trim();
                                empuser.PassCode = strEmpCode;
                            }
                            else
                            {
                                empuser.UserCode = dtMat.GetValue("UserCode", i);
                                empuser.UserID = dtMat.GetValue("UserCode", i);
                                empuser.PassCode = dtMat.GetValue("Password", i);
                            }

                            employee.EmpID = strEmpCode.Trim();
                            EmpID = strEmpCode.Trim();
                            employee.CreateDate = DateTime.Now;
                            employee.CreatedBy = oCompany.UserName;
                            employee.MstUsers.Add(empuser);
                            dbHrPayroll.MstEmployee.InsertOnSubmit(employee);
                        }
                        #region Get Employee Payroll And unlocked Period
                        var oPeriod = (from p in dbHrPayroll.CfgPeriodDates
                                       where employee.PayrollID == p.PayrollId
                                       && p.FlgLocked == false
                                       select p).FirstOrDefault();
                        if (employee != null && employee.PayrollID > 0)
                        {
                            ProcessedSalary = (from s in dbHrPayroll.TrnsSalaryProcessRegister
                                               where s.EmpID == employee.ID
                                               && s.PayrollID == employee.PayrollID
                                               && s.PayrollPeriodID == oPeriod.ID
                                               select s).FirstOrDefault();
                        }
                        #endregion
                        decOldBasicSalary = Convert.ToDecimal(employee.BasicSalary);
                        decOldGrosssalary = Convert.ToDecimal(employee.GrossSalary);
                        intPayrollID = Convert.ToInt32(employee.PayrollID);

                        employee.UpdateDate = DateTime.Now;
                        employee.UpdatedBy = oCompany.UserName;
                        if (!String.IsNullOrEmpty(dtMat.GetValue("EmpActive", i)))
                        {
                            string value = dtMat.GetValue("EmpActive", i);
                            if (value == "Y")
                            {
                                employee.FlgActive = true;
                            }
                            if (value == "N")
                            {
                                employee.FlgActive = false;
                            }
                        }
                        else
                        {
                            //employee.FlgActive = true;
                        }
                        if (!String.IsNullOrEmpty(dtMat.GetValue("OverTime", i)))
                        {
                            string value = dtMat.GetValue("OverTime", i);
                            if (value == "Y")
                            {
                                employee.FlgOTApplicable = true;
                            }
                            if (value == "N")
                            {
                                employee.FlgOTApplicable = false;
                            }
                        }
                        else
                        {
                            //employee.FlgOTApplicable = true;
                        }
                        if (!String.IsNullOrEmpty(dtMat.GetValue("Taxable", i)))
                        {
                            string value = dtMat.GetValue("Taxable", i);
                            if (value == "Y")
                            {
                                employee.FlgTax = true;
                            }
                            if (value == "N")
                            {
                                employee.FlgTax = false;
                            }
                        }
                        else
                        {
                            //employee.FlgTax = true;
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("CostCenter", i)))
                        {
                            string value = dtMat.GetValue("CostCenter", i);
                            string strSql = "Select \"PrcCode\", \"PrcName\" From \"OPRC\"";
                            SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                            oRecSet.DoQuery(strSql);
                            List<string> CostCentersCode = new List<string>();
                            while (oRecSet.EoF == false)
                            {
                                CostCentersCode.Add(Convert.ToString(oRecSet.Fields.Item("PrcCode").Value));
                                oRecSet.MoveNext();
                            }
                            if (CostCentersCode.Contains(value))
                            {
                                employee.CostCenter = value.Trim();
                            }
                            else
                            {
                                oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + "CostCenter Not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            }
                        }
                        else
                        {
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("FirstName", i)))
                        {
                            employee.FirstName = dtMat.GetValue("FirstName", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("MiddleName", i)))
                        {
                            employee.MiddleName = dtMat.GetValue("MiddleName", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("LastName", i)))
                        {
                            employee.LastName = dtMat.GetValue("LastName", i);
                        }

                        if (!string.IsNullOrEmpty(dtMat.GetValue("Initials", i)))
                        {
                            employee.Initials = dtMat.GetValue("Initials", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("NamePrefix", i)))
                        {
                            employee.NamePrefix = dtMat.GetValue("NamePrefix", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("OfficePhone", i)))
                        {
                            employee.OfficePhone = dtMat.GetValue("OfficePhone", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("OfficeExtension", i)))
                        {
                            employee.OfficeExtension = dtMat.GetValue("OfficeExtension", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("OfficeMobile", i)))
                        {
                            employee.OfficeMobile = dtMat.GetValue("OfficeMobile", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("HomePhone", i)))
                        {
                            employee.HomePhone = dtMat.GetValue("HomePhone", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("Fax", i)))
                        {
                            employee.Fax = dtMat.GetValue("Fax", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("OfficeEmail", i)))
                        {
                            employee.OfficeEmail = dtMat.GetValue("OfficeEmail", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("AccountTitle", i)))
                        {
                            employee.AccountTitle = dtMat.GetValue("AccountTitle", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("BankName", i)))
                        {
                            employee.BankName = dtMat.GetValue("BankName", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("BranchName", i)))
                        {
                            employee.BranchName = dtMat.GetValue("BranchName", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("AccountNo", i)))
                        {
                            employee.AccountNo = dtMat.GetValue("AccountNo", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("FatherName", i)))
                        {
                            employee.FatherName = dtMat.GetValue("FatherName", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("MotherName", i)))
                        {
                            employee.MotherName = dtMat.GetValue("MotherName", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("SocialSecurityNo", i)))
                        {
                            employee.SocialSecurityNo = dtMat.GetValue("SocialSecurityNo", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("Nationality", i)))
                        {
                            employee.Nationality = dtMat.GetValue("Nationality", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("PassportNo", i)))
                        {
                            employee.PassportNo = dtMat.GetValue("PassportNo", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("IncomeTaxNo", i)))
                        {
                            employee.IncomeTaxNo = dtMat.GetValue("IncomeTaxNo", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("IDNo", i)))
                        {
                            string strNicNumber = dtMat.GetValue("IDNo", i);

                            #region NIC Duplication Check

                            var objOEmployee = (from p in dbHrPayroll.MstEmployee
                                                where p.EmpID == strEmpCode
                                                select p).FirstOrDefault();
                            if (objOEmployee != null)
                            {
                                if (objOEmployee.IDNo == null)
                                {
                                    employee.IDNo = dtMat.GetValue("IDNo", i);
                                }
                                else if (objOEmployee.IDNo == strNicNumber.Trim())
                                {
                                    employee.IDNo = Convert.ToString(objOEmployee.IDNo);
                                }
                                else if (objOEmployee.IDNo != strNicNumber.Trim())
                                {
                                    int oNICcnt = (from p in dbHrPayroll.MstEmployee
                                                   where p.IDNo == strNicNumber.Trim()
                                                   select p).Count();
                                    if (oNICcnt > 0)
                                    {
                                        oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + strEmpCode + ": Provided NIC Duplicate.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    }
                                    else
                                    {
                                        employee.IDNo = dtMat.GetValue("IDNo", i);
                                    }
                                }
                            }
                            else
                            {
                                int oNICcnt = (from p in dbHrPayroll.MstEmployee
                                               where p.IDNo == strNicNumber.Trim()
                                               select p).Count();
                                if (oNICcnt > 0)
                                {
                                    oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + strEmpCode + ": Provided NIC Duplicate.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                }
                                else
                                {
                                    employee.IDNo = dtMat.GetValue("IDNo", i);
                                }
                            }
                            #endregion

                            #region NIC Duplication Check Old
                            //var objOemployee = (from p in dbHrPayroll.MstEmployee where p.EmpID == strCode select p).FirstOrDefault();

                            //if (objOemployee.IDNo == strNicNumber.Trim())
                            //{
                            //    employee.IDNo = Convert.ToString(objOemployee.IDNo);
                            //}
                            //else if (objOemployee.IDNo == null)
                            //{
                            //    int oNICcnt = (from p in dbHrPayroll.MstEmployee where p.IDNo == strNicNumber.Trim() select p).Count();
                            //    if (oNICcnt > 0)
                            //    {
                            //        oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + strCode + ": Provided NIC Duplicate / Not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            //    }
                            //    else
                            //    {
                            //        employee.IDNo = dtMat.GetValue("IDNo", i);
                            //    }
                            //}
                            //else
                            //{
                            //    oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + strCode + ": Provided NIC Duplicate.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            //    return;
                            //}
                            #endregion

                        }

                        if (!string.IsNullOrEmpty(dtMat.GetValue("IDIssuedBy", i)))
                        {
                            employee.IDIssuedBy = dtMat.GetValue("IDIssuedBy", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("IDPlaceofIssue", i)))
                        {
                            employee.IDPlaceofIssue = dtMat.GetValue("IDPlaceofIssue", i);
                        }
                        // Arabic Values
                        if (!string.IsNullOrEmpty(dtMat.GetValue("EnglishNameH", i)))
                        {
                            employee.EnglishName = dtMat.GetValue("EnglishNameH", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("ArabicNameH", i)))
                        {
                            employee.ArabicName = dtMat.GetValue("ArabicNameH", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("PassportExpiryDateH", i)))
                        {
                            employee.PassportExpiryDt = dtMat.GetValue("PassportExpiryDateH", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("IDExpiryDateH", i)))
                        {
                            employee.IDExpiryDt = dtMat.GetValue("IDExpiryDateH", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("MedicalCardExpiryDateH", i)))
                        {
                            employee.MedicalCardExpDt = dtMat.GetValue("MedicalCardExpiryDateH", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("DrvLicCompletionDateH", i)))
                        {
                            employee.DrvLicCompletionDt = dtMat.GetValue("DrvLicCompletionDateH", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("DrvLicLastDateH", i)))
                        {
                            employee.DrvLicLastDt = dtMat.GetValue("DrvLicLastDateH", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("DrvLicReleaseDateH", i)))
                        {
                            employee.DrvLicReleaseDt = dtMat.GetValue("DrvLicReleaseDateH", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("VisaNo", i)))
                        {
                            employee.VisaNo = dtMat.GetValue("VisaNo", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("IqamaProfessional", i)))
                        {
                            employee.IqamaProfessional = dtMat.GetValue("IqamaProfessional", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("BankCardExpiry", i)))
                        {
                            employee.BankCardExpiryDt = dtMat.GetValue("BankCardExpiry", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("ImagePath", i)))
                        {
                            employee.ImgPath = dtMat.GetValue("ImagePath", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("SortOrder", i)))
                        {
                            employee.SortOrder = Convert.ToInt32(dtMat.GetValue("SortOrder", i));
                        }
                        //End of Arabic Values
                        if (dtMat.GetValue("DOB", i) != "") employee.DOB = DateTime.ParseExact(dtMat.GetValue("DOB", i), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        if (dtMat.GetValue("JoiningDate", i) != "") employee.JoiningDate = DateTime.ParseExact(dtMat.GetValue("JoiningDate", i), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        if (dtMat.GetValue("EffectiveDate", i) != "") employee.EffectiveDate = DateTime.ParseExact(dtMat.GetValue("EffectiveDate", i), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        if (dtMat.GetValue("PassportDateofIssue", i) != "") employee.PassportDateofIssue = DateTime.ParseExact(dtMat.GetValue("PassportDateofIssue", i), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        if (dtMat.GetValue("PassportExpiryDate", i) != "") employee.PassportExpiryDate = DateTime.ParseExact(dtMat.GetValue("PassportExpiryDate", i), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        if (dtMat.GetValue("IDDateofIssue", i) != "") employee.IDDateofIssue = DateTime.ParseExact(dtMat.GetValue("IDDateofIssue", i), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        if (dtMat.GetValue("IDExpiryDate", i) != "") employee.IDExpiryDate = DateTime.ParseExact(dtMat.GetValue("IDExpiryDate", i), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                        int lnkcnt = 0;
                        string strcode = "";
                        //Job Title
                        if (!string.IsNullOrEmpty(dtMat.GetValue("JobTitle", i)))
                        {
                            strcode = dtMat.GetValue("JobTitle", i);
                            var objJobTitle = (from p in dbHrPayroll.MstJobTitle where p.Name == strcode select p).FirstOrDefault();
                            if (objJobTitle != null)
                            {
                                employee.JobTitle = Convert.ToString(objJobTitle.Id);
                            }
                            else
                            {
                                oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + "Provided JobTitle Not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            }
                        }
                        //Designation
                        if (dtMat.GetValue("DesignationName", i) != "")
                        {
                            strcode = dtMat.GetValue("DesignationName", i);
                            lnkcnt = (from p in dbHrPayroll.MstDesignation where p.Name == strcode select p).Count();
                            if (lnkcnt > 0)
                            {

                                //employee.MstDesignation = (from p in dbHrPayroll.MstDesignation where p.Name == strcode select p).Single();
                                employee.MstDesignation = (from p in dbHrPayroll.MstDesignation where p.Name == strcode select p).FirstOrDefault();
                                employee.DesignationName = strcode;
                            }
                            else
                            {
                                oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + "Provided Designation Not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            }
                        }
                        //Position
                        if (dtMat.GetValue("PositionName", i) != "")
                        {
                            strcode = dtMat.GetValue("PositionName", i);
                            lnkcnt = (from p in dbHrPayroll.MstPosition where p.Name == strcode select p).Count();
                            if (lnkcnt > 0)
                            {
                                //MstPosition pos = (from p in dbHrPayroll.MstPosition where p.Name == strcode select p).Single();
                                MstPosition pos = (from p in dbHrPayroll.MstPosition where p.Name == strcode select p).FirstOrDefault();
                                employee.PositionName = strcode;
                                employee.PositionID = pos.Id;
                            }
                            else
                            {
                                oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + "Provided Position Not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            }
                        }
                        //Department
                        #region Department  update validation
                        if (ProcessedSalary != null)
                        {
                            if (ProcessedSalary.EmpID != null && ProcessedSalary.PayrollID > 0)
                            {
                                strcode = dtMat.GetValue("DepartmentName", i);
                                var PostedSalary = (from je in dbHrPayroll.TrnsJE
                                                    where je.ID == ProcessedSalary.JENum
                                                    select je).FirstOrDefault();
                                if (PostedSalary != null)
                                {
                                    if (PostedSalary.FlgPosted.GetValueOrDefault() != true)
                                    {
                                        if (strcode.Trim() != employee.DepartmentName.Trim())
                                        {
                                            oApplication.StatusBar.SetText("Department can't be updated of Salary processed Employee '" + employee.EmpID + "'", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                        }
                                    }
                                    else
                                    {
                                        if (dtMat.GetValue("DepartmentName", i) != "")
                                        {
                                            strcode = dtMat.GetValue("DepartmentName", i);
                                            lnkcnt = (from p in dbHrPayroll.MstDepartment
                                                      where p.Code == strcode
                                                      select p).Count();
                                            if (lnkcnt > 0)
                                            {
                                                var oDept = (from p in dbHrPayroll.MstDepartment
                                                             where p.Code == strcode
                                                             select p).FirstOrDefault();
                                                employee.MstDepartment = oDept;
                                                employee.DepartmentName = oDept.DeptName;
                                            }
                                            else
                                            {
                                                oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + "Provided Department Not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                            }
                                        }
                                    }
                                }
                                //else if (ProcessedSalary != null && PostedSalary == null)
                                else if (ProcessedSalary.EmpID != null && ProcessedSalary.PayrollID > 0 && PostedSalary == null)
                                {
                                    if (strcode.Trim() != employee.DepartmentName.Trim())
                                    {
                                        oApplication.StatusBar.SetText("Department can't be updated of Salary processed Employee '" + employee.EmpID + "'", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    }

                                }
                                else
                                {
                                    if (dtMat.GetValue("DepartmentName", i) != "")
                                    {
                                        strcode = dtMat.GetValue("DepartmentName", i);
                                        lnkcnt = (from p in dbHrPayroll.MstDepartment where p.Code == strcode select p).Count();
                                        if (lnkcnt > 0)
                                        {
                                            var oDept = (from p in dbHrPayroll.MstDepartment where p.Code == strcode select p).FirstOrDefault();
                                            employee.MstDepartment = oDept;
                                            employee.DepartmentName = oDept.DeptName;
                                        }
                                        else
                                        {
                                            oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + "Provided Department Not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (dtMat.GetValue("DepartmentName", i) != "")
                                {
                                    strcode = dtMat.GetValue("DepartmentName", i);
                                    lnkcnt = (from p in dbHrPayroll.MstDepartment where p.Code == strcode select p).Count();
                                    if (lnkcnt > 0)
                                    {
                                        var oDept = (from p in dbHrPayroll.MstDepartment where p.Code == strcode select p).FirstOrDefault();
                                        employee.MstDepartment = oDept;
                                        employee.DepartmentName = oDept.DeptName;
                                    }
                                    else
                                    {
                                        oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + "Provided Department Not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (dtMat.GetValue("DepartmentName", i) != "")
                            {
                                strcode = dtMat.GetValue("DepartmentName", i);
                                lnkcnt = (from p in dbHrPayroll.MstDepartment where p.Code == strcode select p).Count();
                                if (lnkcnt > 0)
                                {
                                    var oDept = (from p in dbHrPayroll.MstDepartment where p.Code == strcode select p).FirstOrDefault();
                                    employee.MstDepartment = oDept;
                                    employee.DepartmentName = oDept.DeptName;
                                }
                                else
                                {
                                    oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + "Provided Department Not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                }
                            }
                        }
                        #endregion
                        //if (dtMat.GetValue("DepartmentName", i) != "")
                        //{
                        //    strcode = dtMat.GetValue("DepartmentName", i);
                        //    lnkcnt = (from p in dbHrPayroll.MstDepartment where p.Code == strcode select p).Count();
                        //    if (lnkcnt > 0)
                        //    {
                        //        var oDept = (from p in dbHrPayroll.MstDepartment where p.Code == strcode select p).FirstOrDefault();
                        //        employee.MstDepartment = oDept;
                        //        employee.DepartmentName = oDept.DeptName;
                        //    }
                        //    else
                        //    {
                        //        oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + "Provided Department Not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        //    }
                        //}

                        //Location
                        #region Location  update validation
                        if (ProcessedSalary != null)
                        {
                            if (ProcessedSalary.EmpID != null && ProcessedSalary.PayrollID > 0)
                            {
                                strcode = dtMat.GetValue("LocationName", i);
                                var PostedSalary = (from je in dbHrPayroll.TrnsJE
                                                    where je.ID == ProcessedSalary.JENum
                                                    select je).FirstOrDefault();
                                if (PostedSalary != null)
                                {
                                    if (PostedSalary.FlgPosted.GetValueOrDefault() != true)
                                    {
                                        if (strcode.Trim() != employee.LocationName.Trim())
                                        {
                                            oApplication.StatusBar.SetText("Location can't be updated of Salary processed Employee '" + employee.EmpID + "'", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                        }
                                    }
                                    else
                                    {
                                        if (dtMat.GetValue("LocationName", i) != "")
                                        {
                                            strcode = dtMat.GetValue("LocationName", i);
                                            lnkcnt = (from p in dbHrPayroll.MstLocation where p.Name == strcode select p).Count();
                                            if (lnkcnt > 0)
                                            {
                                                //employee.MstLocation = (from p in dbHrPayroll.MstLocation where p.Name == strcode select p).Single();
                                                employee.MstLocation = (from p in dbHrPayroll.MstLocation where p.Name == strcode select p).FirstOrDefault();
                                                employee.LocationName = strcode;
                                            }
                                            else
                                            {
                                                oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + "Provided Location Not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                            }
                                        }
                                    }
                                }
                                //else if (ProcessedSalary != null && PostedSalary == null)
                                else if (ProcessedSalary.EmpID != null && ProcessedSalary.PayrollID > 0 && PostedSalary == null)
                                {
                                    if (strcode.Trim() != employee.LocationName.Trim())
                                    {
                                        oApplication.StatusBar.SetText("Location can't be updated of Salary processed Employee '" + employee.EmpID + "'", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    }
                                }
                                else
                                {
                                    if (dtMat.GetValue("LocationName", i) != "")
                                    {
                                        strcode = dtMat.GetValue("LocationName", i);
                                        lnkcnt = (from p in dbHrPayroll.MstLocation where p.Name == strcode select p).Count();
                                        if (lnkcnt > 0)
                                        {
                                            //employee.MstLocation = (from p in dbHrPayroll.MstLocation where p.Name == strcode select p).Single();
                                            employee.MstLocation = (from p in dbHrPayroll.MstLocation where p.Name == strcode select p).FirstOrDefault();
                                            employee.LocationName = strcode;
                                        }
                                        else
                                        {
                                            oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + "Provided Location Not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (dtMat.GetValue("LocationName", i) != "")
                                {
                                    strcode = dtMat.GetValue("LocationName", i);
                                    lnkcnt = (from p in dbHrPayroll.MstLocation where p.Name == strcode select p).Count();
                                    if (lnkcnt > 0)
                                    {
                                        //employee.MstLocation = (from p in dbHrPayroll.MstLocation where p.Name == strcode select p).Single();
                                        employee.MstLocation = (from p in dbHrPayroll.MstLocation where p.Name == strcode select p).FirstOrDefault();
                                        employee.LocationName = strcode;
                                    }
                                    else
                                    {
                                        oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + "Provided Location Not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (dtMat.GetValue("LocationName", i) != "")
                            {
                                strcode = dtMat.GetValue("LocationName", i);
                                lnkcnt = (from p in dbHrPayroll.MstLocation where p.Name == strcode select p).Count();
                                if (lnkcnt > 0)
                                {
                                    //employee.MstLocation = (from p in dbHrPayroll.MstLocation where p.Name == strcode select p).Single();
                                    employee.MstLocation = (from p in dbHrPayroll.MstLocation where p.Name == strcode select p).FirstOrDefault();
                                    employee.LocationName = strcode;
                                }
                                else
                                {
                                    oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + "Provided Location Not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                }
                            }
                        }
                        #endregion
                        //if (dtMat.GetValue("LocationName", i) != "")
                        //{
                        //    strcode = dtMat.GetValue("LocationName", i);
                        //    lnkcnt = (from p in dbHrPayroll.MstLocation where p.Name == strcode select p).Count();
                        //    if (lnkcnt > 0)
                        //    {
                        //        //employee.MstLocation = (from p in dbHrPayroll.MstLocation where p.Name == strcode select p).Single();
                        //        employee.MstLocation = (from p in dbHrPayroll.MstLocation where p.Name == strcode select p).FirstOrDefault();
                        //        employee.LocationName = strcode;
                        //    }
                        //    else
                        //    {
                        //        oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + "Provided Location Not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        //    }
                        //}
                        //Branch
                        if (dtMat.GetValue("BranchName", i) != "")
                        {
                            strcode = dtMat.GetValue("BranchName", i);
                            lnkcnt = (from p in dbHrPayroll.MstBranches where p.Name == strcode select p).Count();
                            if (lnkcnt > 0)
                            {
                                //MstBranches brn = (from p in dbHrPayroll.MstBranches where p.Name == strcode select p).Single();
                                MstBranches brn = (from p in dbHrPayroll.MstBranches where p.Name == strcode select p).FirstOrDefault();
                                employee.BranchID = brn.Id;
                                employee.BranchName = strcode;
                            }
                            else
                            {
                                oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + "Provided Branch Not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            }
                        }

                        //Payroll
                        #region Payroll  update validation
                        if (ProcessedSalary != null)
                        {
                            if (ProcessedSalary.EmpID != null && ProcessedSalary.PayrollID > 0)
                            {
                                strcode = dtMat.GetValue("PayrollName", i);
                                var PostedSalary = (from je in dbHrPayroll.TrnsJE
                                                    where je.ID == ProcessedSalary.JENum
                                                    select je).FirstOrDefault();
                                if (PostedSalary != null)
                                {
                                    if (PostedSalary.FlgPosted.GetValueOrDefault() != true)
                                    {
                                        if (strcode.Trim() != employee.PayrollName.Trim())
                                        {
                                            oApplication.StatusBar.SetText("Payroll can't be updated of Salary processed Employee '" + employee.EmpID + "'", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                        }
                                    }
                                    else
                                    {
                                        if (dtMat.GetValue("PayrollName", i) != "")
                                        {
                                            strcode = dtMat.GetValue("PayrollName", i);
                                            lnkcnt = (from p in dbHrPayroll.CfgPayrollDefination where p.PayrollName == strcode select p).Count();
                                            if (lnkcnt > 0)
                                            {
                                                //employee.CfgPayrollDefination = (from p in dbHrPayroll.CfgPayrollDefination where p.PayrollName == strcode select p).Single();
                                                employee.CfgPayrollDefination = (from p in dbHrPayroll.CfgPayrollDefination where p.PayrollName == strcode select p).FirstOrDefault();
                                                employee.PayrollName = strcode;
                                            }
                                            else
                                            {
                                                oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + "Provided Payroll Not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                            }
                                        }
                                    }
                                }
                                //else if (ProcessedSalary != null && PostedSalary == null)
                                else if (ProcessedSalary.EmpID != null && ProcessedSalary.PayrollID > 0 && PostedSalary == null)
                                {
                                    if (strcode.Trim() != employee.PayrollName.Trim())
                                    {
                                        oApplication.StatusBar.SetText("Payroll can't be updated of Salary processed Employee '" + employee.EmpID + "'", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    }
                                }
                                else
                                {
                                    if (dtMat.GetValue("PayrollName", i) != "")
                                    {
                                        strcode = dtMat.GetValue("PayrollName", i);
                                        lnkcnt = (from p in dbHrPayroll.CfgPayrollDefination where p.PayrollName == strcode select p).Count();
                                        if (lnkcnt > 0)
                                        {
                                            //employee.CfgPayrollDefination = (from p in dbHrPayroll.CfgPayrollDefination where p.PayrollName == strcode select p).Single();
                                            employee.CfgPayrollDefination = (from p in dbHrPayroll.CfgPayrollDefination where p.PayrollName == strcode select p).FirstOrDefault();
                                            employee.PayrollName = strcode;
                                        }
                                        else
                                        {
                                            oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + "Provided Payroll Not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (dtMat.GetValue("PayrollName", i) != "")
                                {
                                    strcode = dtMat.GetValue("PayrollName", i);
                                    lnkcnt = (from p in dbHrPayroll.CfgPayrollDefination where p.PayrollName == strcode select p).Count();
                                    if (lnkcnt > 0)
                                    {
                                        //employee.CfgPayrollDefination = (from p in dbHrPayroll.CfgPayrollDefination where p.PayrollName == strcode select p).Single();
                                        employee.CfgPayrollDefination = (from p in dbHrPayroll.CfgPayrollDefination where p.PayrollName == strcode select p).FirstOrDefault();
                                        employee.PayrollName = strcode;
                                    }
                                    else
                                    {
                                        oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + "Provided Payroll Not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (dtMat.GetValue("PayrollName", i) != "")
                            {
                                strcode = dtMat.GetValue("PayrollName", i);
                                lnkcnt = (from p in dbHrPayroll.CfgPayrollDefination where p.PayrollName == strcode select p).Count();
                                if (lnkcnt > 0)
                                {
                                    //employee.CfgPayrollDefination = (from p in dbHrPayroll.CfgPayrollDefination where p.PayrollName == strcode select p).Single();
                                    employee.CfgPayrollDefination = (from p in dbHrPayroll.CfgPayrollDefination where p.PayrollName == strcode select p).FirstOrDefault();
                                    employee.PayrollName = strcode;
                                }
                                else
                                {
                                    oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + "Provided Payroll Not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                }
                            }
                        }
                        #endregion
                        //if (dtMat.GetValue("PayrollName", i) != "")
                        //{
                        //    strcode = dtMat.GetValue("PayrollName", i);
                        //    lnkcnt = (from p in dbHrPayroll.CfgPayrollDefination where p.PayrollName == strcode select p).Count();
                        //    if (lnkcnt > 0)
                        //    {
                        //        //employee.CfgPayrollDefination = (from p in dbHrPayroll.CfgPayrollDefination where p.PayrollName == strcode select p).Single();
                        //        employee.CfgPayrollDefination = (from p in dbHrPayroll.CfgPayrollDefination where p.PayrollName == strcode select p).FirstOrDefault();
                        //        employee.PayrollName = strcode;
                        //    }
                        //    else
                        //    {
                        //        oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + "Provided Payroll Not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        //    }
                        //}
                        //Basic Salary
                        //employee.BasicSalary = 0.00M;
                        #region Basic Salary update validation
                        if (ProcessedSalary != null)
                        {
                            if (ProcessedSalary.EmpID != null && ProcessedSalary.PayrollID > 0)
                            {
                                Decimal decBasicSalary = 0;
                                if (dtMat.GetValue("BasicSalary", i) != "")
                                {
                                    decBasicSalary = Convert.ToDecimal(dtMat.GetValue("BasicSalary", i));
                                }

                                var PostedSalary = (from je in dbHrPayroll.TrnsJE
                                                    where je.ID == ProcessedSalary.JENum
                                                    select je).FirstOrDefault();
                                if (PostedSalary != null)
                                {
                                    if (PostedSalary.FlgPosted.GetValueOrDefault() != true)
                                    {
                                        if (decBasicSalary != employee.BasicSalary)
                                        {
                                            oApplication.StatusBar.SetText("Basic Salary can't be updated of Salary processed Employee '" + employee.EmpID + "'", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                        }
                                    }
                                    else
                                    {
                                        if (dtMat.GetValue("BasicSalary", i) != "")
                                        {
                                            try
                                            {
                                                employee.BasicSalary = Convert.ToDecimal(dtMat.GetValue("BasicSalary", i));
                                            }
                                            catch { }
                                        }
                                    }
                                }
                                //else if (ProcessedSalary != null && PostedSalary == null)
                                else if (ProcessedSalary.EmpID != null && ProcessedSalary.PayrollID > 0 && PostedSalary == null)
                                {
                                    if (decBasicSalary != employee.BasicSalary)
                                    {
                                        oApplication.StatusBar.SetText("Basic Salary can't be updated of Salary processed Employee '" + employee.EmpID + "'", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    }
                                }
                                else
                                {
                                    if (dtMat.GetValue("BasicSalary", i) != "")
                                    {
                                        try
                                        {
                                            employee.BasicSalary = Convert.ToDecimal(dtMat.GetValue("BasicSalary", i));
                                        }
                                        catch { }
                                    }
                                }
                            }
                            else
                            {
                                if (dtMat.GetValue("BasicSalary", i) != "")
                                {
                                    try
                                    {
                                        employee.BasicSalary = Convert.ToDecimal(dtMat.GetValue("BasicSalary", i));
                                    }
                                    catch { }
                                }
                            }
                        }
                        else
                        {
                            if (dtMat.GetValue("BasicSalary", i) != "")
                            {
                                try
                                {
                                    employee.BasicSalary = Convert.ToDecimal(dtMat.GetValue("BasicSalary", i));
                                }
                                catch { }
                            }
                        }
                        #endregion
                        //if (dtMat.GetValue("BasicSalary", i) != "")
                        //{
                        //    try
                        //    {
                        //        employee.BasicSalary = Convert.ToDecimal(dtMat.GetValue("BasicSalary", i));
                        //    }
                        //    catch { }
                        //}
                        //SalaryCurrency
                        if (dtMat.GetValue("SalaryCurrency", i) != "")
                        {
                            string currency = dtMat.GetValue("SalaryCurrency", i);
                            if (ValueFoundinMstLOV(currency))
                            {
                                employee.SalaryCurrency = dtMat.GetValue("SalaryCurrency", i);
                            }
                            else
                            {
                                oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + "Provided SalaryCurrency Not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            }
                        }

                        //PaymentMode
                        if (dtMat.GetValue("PaymentMode", i) != "")
                        {
                            string PaymentMode = dtMat.GetValue("PaymentMode", i);
                            if (ValueFoundinMstLOV(PaymentMode))
                            {
                                employee.PaymentMode = dtMat.GetValue("PaymentMode", i);
                            }
                            else
                            {
                                oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + "Provided PaymentMode Not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            }
                        }

                        //AccountType
                        if (!string.IsNullOrEmpty(dtMat.GetValue("AccountType", i)))
                        {
                            strcode = dtMat.GetValue("AccountType", i);
                            if (ValueFoundinMstLOV(strcode))
                            {
                                employee.AccountType = dtMat.GetValue("AccountType", i);
                            }
                            else
                            {
                                oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + "Provided AccountType Not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            }
                        }
                        //DefaultDayOff
                        if (!string.IsNullOrEmpty(dtMat.GetValue("OffDay", i)))
                        {
                            strcode = dtMat.GetValue("OffDay", i);
                            if (ValueFoundinMstLOV(strcode))
                            {
                                employee.DefaultOffDay = dtMat.GetValue("OffDay", i);
                            }
                            else
                            {
                                oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + "Provided AccountType Not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            }
                        }
                        if (!String.IsNullOrEmpty(dtMat.GetValue("OffDayApplicable", i)))
                        {
                            string value = dtMat.GetValue("OffDayApplicable", i);
                            if (value == "Y")
                            {
                                employee.FlgOffDayApplicable = true;
                            }
                            if (value == "N")
                            {
                                employee.FlgOffDayApplicable = false;
                            }
                        }
                        //ReligionID
                        if (!string.IsNullOrEmpty(dtMat.GetValue("ReligionID", i)))
                        {
                            strcode = dtMat.GetValue("ReligionID", i);
                            if (ValueFoundinMstLOV(strcode))
                            {
                                employee.ReligionID = dtMat.GetValue("ReligionID", i);
                            }
                            else
                            {
                                oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + "Provided ReligionID Not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            }
                        }
                        //MartialStatusID
                        if (!string.IsNullOrEmpty(dtMat.GetValue("MartialStatusID", i)))
                        {
                            strcode = dtMat.GetValue("MartialStatusID", i);
                            if (ValueFoundinMstLOV(strcode))
                            {
                                employee.MartialStatusID = dtMat.GetValue("MartialStatusID", i);
                            }
                            else
                            {
                                oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + "Provided MartialStatusID Not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            }
                        }

                        //Gender
                        if (!string.IsNullOrEmpty(dtMat.GetValue("Gender", i)))
                        {
                            strcode = dtMat.GetValue("Gender", i);
                            if (ValueFoundinMstLOV(strcode))
                            {
                                employee.GenderID = dtMat.GetValue("Gender", i);
                            }
                            else
                            {
                                oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + "Provided Gender type Not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            }

                        }
                        //Employee COntract Type
                        if (!string.IsNullOrEmpty(dtMat.GetValue("ContractType", i)))
                        {
                            strcode = dtMat.GetValue("ContractType", i);
                            if (ValueFoundinMstLOV(strcode))
                            {
                                employee.EmployeeContractType = dtMat.GetValue("ContractType", i);
                            }
                            else
                            {
                                oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + "Provided ContractType Not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            }
                        }
                        //Manager
                        if (dtMat.GetValue("Manager", i) != "")
                        {
                            strcode = dtMat.GetValue("Manager", i);
                            lnkcnt = (from p in dbHrPayroll.MstEmployee where p.EmpID == strcode select p).Count();
                            if (lnkcnt > 0)
                            {
                                MstEmployee emp = (from p in dbHrPayroll.MstEmployee where p.EmpID == strcode select p).Single();
                                employee.Manager = emp.ID;
                            }
                            else
                            {
                                oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + "Provided Manager Not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            }
                        }
                        //Fixed GOSI Salary
                        if (!string.IsNullOrEmpty(dtMat.GetValue("FixedGosiSalary", i)))
                        {
                            strcode = dtMat.GetValue("FixedGosiSalary", i);
                            try
                            {
                                employee.GosiSalary = string.IsNullOrEmpty(strcode) ? 0 : Convert.ToDecimal(strcode);
                            }
                            catch
                            {

                            }
                        }
                        //Variable GOSI Salary
                        if (!string.IsNullOrEmpty(dtMat.GetValue("VariableGosiSalary", i)))
                        {
                            strcode = dtMat.GetValue("VariableGosiSalary", i);
                            try
                            {
                                employee.GosiSalaryV = string.IsNullOrEmpty(strcode) ? 0 : Convert.ToDecimal(strcode);
                            }
                            catch
                            {

                            }
                        }
                        ////Working Country
                        //if (!string.IsNullOrEmpty(dtMat.GetValue("WorkingCountryCode", i)))
                        //{
                        //    employee.WACountry = dtMat.GetValue("WorkingCountryCode", i);
                        //}
                        ////Home Country
                        //if (!string.IsNullOrEmpty(dtMat.GetValue("HomeCountryCode", i)))
                        //{
                        //    employee.HACountry = dtMat.GetValue("HomeCountryCode", i);
                        //}
                        if (!String.IsNullOrEmpty(dtMat.GetValue("EmailSlip", i)))
                        {
                            string value = dtMat.GetValue("EmailSlip", i);
                            if (value == "Y")
                            {
                                employee.FlgEmail = true;
                            }
                            if (value == "N")
                            {
                                employee.FlgEmail = false;
                            }
                        }
                        if (!String.IsNullOrEmpty(dtMat.GetValue("Sandwich", i)))
                        {
                            string value = dtMat.GetValue("Sandwich", i);
                            if (value == "Y")
                            {
                                employee.FlgSandwich = true;
                            }
                            if (value == "N")
                            {
                                employee.FlgSandwich = false;
                            }
                        }
                        if (!String.IsNullOrEmpty(dtMat.GetValue("BlackLists", i)))
                        {
                            string value = dtMat.GetValue("BlackLists", i);
                            if (value == "Y")
                            {
                                employee.FlgBlackListed = true;
                            }
                            if (value == "N")
                            {
                                employee.FlgBlackListed = false;
                            }
                        }
                        if (!String.IsNullOrEmpty(dtMat.GetValue("CompanyResidence", i)))
                        {
                            string value = dtMat.GetValue("CompanyResidence", i);
                            if (value == "Y")
                            {
                                employee.FlgCompanyResidence = true;
                            }
                            if (value == "N")
                            {
                                employee.FlgCompanyResidence = false;
                            }
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("HomeStreet", i)))
                        {
                            employee.HAStreet = dtMat.GetValue("HomeStreet", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("HomeStreetNo", i)))
                        {
                            employee.HAStreetNo = dtMat.GetValue("HomeStreetNo", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("HomeBlock", i)))
                        {
                            employee.HABlock = dtMat.GetValue("HomeBlock", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("HomeOther", i)))
                        {
                            employee.HAOther = dtMat.GetValue("HomeOther", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("HomeZipCode", i)))
                        {
                            employee.HAZipCode = dtMat.GetValue("HomeZipCode", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("HomeCity", i)))
                        {
                            employee.HACity = dtMat.GetValue("HomeCity", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("HomeState", i)))
                        {
                            employee.HAState = dtMat.GetValue("HomeState", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("HomeCountry", i)))
                        {
                            employee.HACountry = dtMat.GetValue("HomeCountry", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("WorkStreet", i)))
                        {
                            employee.WAStreet = dtMat.GetValue("WorkStreet", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("WorkStreetNo", i)))
                        {
                            employee.WAStreetNo = dtMat.GetValue("WorkStreetNo", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("WorkBlock", i)))
                        {
                            employee.WABlock = dtMat.GetValue("WorkBlock", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("WorkOther", i)))
                        {
                            employee.WAOther = dtMat.GetValue("WorkOther", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("WorkZipCode", i)))
                        {
                            employee.WAZipCode = dtMat.GetValue("WorkZipCode", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("WorkCity", i)))
                        {
                            employee.WACity = dtMat.GetValue("WorkCity", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("WorkState", i)))
                        {
                            employee.WAState = dtMat.GetValue("WorkState", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("WorkCountry", i)))
                        {
                            employee.WACountry = dtMat.GetValue("WorkCountry", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("CompanyResidencaAddress", i)))
                        {
                            employee.CompanyAddress = dtMat.GetValue("CompanyResidencaAddress", i);
                        }

                        if (!string.IsNullOrEmpty(dtMat.GetValue("PrimaryPersonName", i)))
                        {
                            employee.PriPersonName = dtMat.GetValue("PrimaryPersonName", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("PrimaryRelation", i)))
                        {
                            employee.PriRelationShip = dtMat.GetValue("PrimaryRelation", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("PrimaryContactLandLine", i)))
                        {
                            employee.PriContactNoLandLine = dtMat.GetValue("PrimaryContactLandLine", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("PrimaryContactMobile", i)))
                        {
                            employee.PriContactNoMobile = dtMat.GetValue("PrimaryContactMobile", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("PrimaryAddress", i)))
                        {
                            employee.PriAddress = dtMat.GetValue("PrimaryAddress", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("PrimaryCity", i)))
                        {
                            employee.PriCity = dtMat.GetValue("PrimaryCity", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("PrimaryState", i)))
                        {
                            employee.PriState = dtMat.GetValue("PrimaryState", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("PrimaryCountry", i)))
                        {
                            employee.PriCountry = dtMat.GetValue("PrimaryCountry", i);
                        }

                        if (!string.IsNullOrEmpty(dtMat.GetValue("SecondaryPersonName", i)))
                        {
                            employee.SecPersonName = dtMat.GetValue("SecondaryPersonName", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("SecondaryRelation", i)))
                        {
                            employee.SecRelationShip = dtMat.GetValue("SecondaryRelation", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("SecondaryContactLandLine", i)))
                        {
                            employee.SecContactNoLandline = dtMat.GetValue("SecondaryContactLandLine", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("SecondaryContactMobile", i)))
                        {
                            employee.SecContactNoMobile = dtMat.GetValue("SecondaryContactMobile", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("SecondaryAddress", i)))
                        {
                            employee.SecAddress = dtMat.GetValue("SecondaryAddress", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("SecondaryCity", i)))
                        {
                            employee.SecCity = dtMat.GetValue("SecondaryCity", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("SecondaryState", i)))
                        {
                            employee.SecState = dtMat.GetValue("SecondaryState", i);
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("SecondaryCountry", i)))
                        {
                            employee.SecCountry = dtMat.GetValue("SecondaryCountry", i);
                        }
                        if (dtMat.GetValue("AdvancePercentage", i) != "")
                        {
                            try
                            {
                                employee.AllowedAdvance = Convert.ToDecimal(dtMat.GetValue("AdvancePercentage", i));
                            }
                            catch { }
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("TranportMode", i)))
                        {
                            strcode = dtMat.GetValue("TranportMode", i);
                            if (ValueFoundinMstLOV(strcode))
                            {
                                employee.TransportMode = dtMat.GetValue("TranportMode", i);
                            }
                            else
                            {
                                oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + "Provided ContractType Not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            }
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("RecruitmentMode", i)))
                        {
                            strcode = dtMat.GetValue("RecruitmentMode", i);
                            if (ValueFoundinMstLOV(strcode))
                            {
                                employee.RecruitmentMode = dtMat.GetValue("RecruitmentMode", i);
                            }
                            else
                            {
                                oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + "Provided ContractType Not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            }
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("InsuranceCategory", i)))
                        {
                            strcode = dtMat.GetValue("InsuranceCategory", i);
                            if (ValueFoundinMstLOV(strcode))
                            {
                                employee.InsuranceCategory = dtMat.GetValue("InsuranceCategory", i);
                            }
                            else
                            {
                                oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + "Provided ContractType Not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            }
                        }
                        //Gratuity Slab
                        if (!string.IsNullOrEmpty(dtMat.GetValue("GratuitySlab", i)))
                        {
                            strcode = dtMat.GetValue("GratuitySlab", i);
                            var oGratuity = (from a in dbHrPayroll.TrnsGratuitySlabs
                                             where a.SlabCode == strcode
                                             select a).FirstOrDefault();
                            if (oGratuity != null)
                            {
                                employee.GratuitySlabs = oGratuity.InternalID;
                            }
                            else
                            {
                                oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + "Provided ContractType Not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            }
                        }
                        //Overtime Slab
                        if (!string.IsNullOrEmpty(dtMat.GetValue("OTSlab", i)))
                        {
                            strcode = dtMat.GetValue("OTSlab", i);
                            var oOTSlab = (from a in dbHrPayroll.TrnsOTSlab
                                           where a.SlabCode == strcode
                                           select a).FirstOrDefault();
                            if (oOTSlab != null)
                            {
                                employee.OTSlabs = oOTSlab.InternalID;
                            }
                            else
                            {
                                oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + "Provided ContractType Not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            }
                        }
                        //Shift Day Slab ShiftDaysSlab
                        if (!string.IsNullOrEmpty(dtMat.GetValue("ShiftDaysSlab", i)))
                        {
                            strcode = dtMat.GetValue("ShiftDaysSlab", i);
                            var oShiftDays = (from a in dbHrPayroll.MstShiftDays
                                              where a.Code == strcode
                                              select a).FirstOrDefault();
                            if (oShiftDays != null)
                            {
                                employee.ShiftDaysCode = oShiftDays.Code;
                            }
                            else
                            {
                                oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + "Provided ContractType Not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            }
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("GrossSalary", i)))
                        {
                            strcode = dtMat.GetValue("GrossSalary", i);

                            if (mFm.IsDecimal(strcode))
                            {
                                employee.GrossSalary = Convert.ToDecimal(strcode);
                            }
                            else
                            {
                                employee.GrossSalary = 0;
                                oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + "Provided ContractType Not Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            }
                        }
                        if (!string.IsNullOrEmpty(dtMat.GetValue("EmpCalender", i)))
                        {
                            string strEmpCalendarCode = dtMat.GetValue("EmpCalender", i);
                            if (!string.IsNullOrEmpty(strEmpCalendarCode))
                            {
                                SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                                string SQLHolidays = "SELECT \"HldCode\", \"Rmrks\" FROM \"HLD1\" WHERE \"HldCode\" = '" + strEmpCalendarCode + "'";
                                oRecSet.DoQuery(SQLHolidays);
                                if (oRecSet.RecordCount > 0)
                                {
                                    employee.EmpCalender = dtMat.GetValue("EmpCalender", i);
                                }
                                else
                                {
                                    employee.EmpCalender = "";
                                    oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee calendar code not foun.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                }
                            }
                        }

                        ErrorAtLine = i + 1;
                        dbHrPayroll.SubmitChanges();
                        #region Element Update
                        if (!flgUpdateCheck)
                        {
                            ds.updateStandardElements(employee.EmpID, true, Program.ConStrHRMS);
                        }
                        else if (flgUpdateCheck)
                        {
                            if (employee.BasicSalary != decOldBasicSalary || employee.GrossSalary != decOldGrosssalary
                                || employee.PayrollID != intPayrollID)
                            {
                                ds.updateStandardElements(employee.EmpID, true, Program.ConStrHRMS);
                            }
                        }
                        #endregion
                    }

                    //dbHrPayroll.SubmitChanges();
                    oApplication.MessageBox("Employee imported successfully");
                    ClearRecords();
                }
                else
                {
                    oApplication.StatusBar.SetText("No data available for import, Please select template.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return;
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + " System Error : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void PostEmployeePerPieceRate()
        {
            int ErrorAtLine = 1;
            string EmpCode = "";
            try
            {
                if (dtMat.Rows.Count > 0)
                {
                    for (int i = 0; i < dtMat.Rows.Count; i++)
                    {
                        ErrorAtLine = i + 1;
                        string EmployeeCode, ItemCode, ItemName, ItemRate;
                        EmployeeCode = dtMat.GetValue("EmpID", i);
                        ItemCode = dtMat.GetValue("ItemCode", i);
                        ItemName = dtMat.GetValue("ItemName", i);
                        ItemRate = dtMat.GetValue("Rate", i);
                        if (!string.IsNullOrEmpty(EmployeeCode))
                        {
                            EmpCode = EmployeeCode;
                            int check = (from a in dbHrPayroll.MstEmployee where a.EmpID == EmployeeCode select a).Count();
                            if (check == 0)
                            {
                                oApplication.StatusBar.SetText("employee not found error at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                return;
                            }
                        }
                        else
                        {
                            oApplication.StatusBar.SetText("EmpCode is empty. at Line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return;
                        }
                        if (!string.IsNullOrEmpty(ItemCode) && !string.IsNullOrEmpty(ItemName))
                        {
                            string strSql = "SELECT ItemCode, ItemName FROM dbo.OITM WHERE ISNULL(U_PerPieceItem,'N') = 'Y' AND ItemCode = '" + ItemCode + "'";
                            SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                            oRecSet.DoQuery(strSql);
                            List<Program.SapItems> oItems = new List<Program.SapItems>();
                            string strItemCode = "", strItemName = "";
                            while (oRecSet.EoF == false)
                            {
                                strItemCode = Convert.ToString(oRecSet.Fields.Item("ItemCode").Value);
                                strItemName = Convert.ToString(oRecSet.Fields.Item("ItemName").Value);
                                if (!string.IsNullOrEmpty(strItemName) && !string.IsNullOrEmpty(strItemCode))
                                {
                                    var oneitem = new Program.SapItems();
                                    oneitem.ItemCode = strItemCode;
                                    oneitem.ItemName = strItemName;
                                    oItems.Add(oneitem);
                                }
                                strItemCode = string.Empty;
                                strItemName = string.Empty;
                                oRecSet.MoveNext();
                            }
                            if (oItems.Count == 0)
                            {
                                oApplication.StatusBar.SetText("item not found error at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                return;
                            }
                        }
                        else
                        {
                            oApplication.StatusBar.SetText("item code is empty error at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return;
                        }
                        if (!string.IsNullOrEmpty(ItemRate))
                        {
                            try
                            {
                                decimal holdvalue = 0M;
                                bool canconvert = false;
                                canconvert = decimal.TryParse(ItemRate, out holdvalue);
                                if (!canconvert)
                                {
                                    oApplication.StatusBar.SetText("item rate doesn't contain numeric value error at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    return;
                                }
                            }
                            catch
                            {
                                oApplication.StatusBar.SetText("item rate doesn't contain numeric value error at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                return;
                            }
                        }
                        else
                        {
                            oApplication.StatusBar.SetText("item rate is empty error at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return;
                        }

                        MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == EmployeeCode select a).FirstOrDefault();
                        if (oEmp == null) continue;
                        int checkdoc = 0;
                        checkdoc = (from a in dbHrPayroll.TrnsEmployeePerPieceRate where a.MstEmployee.EmpID == oEmp.EmpID select a).Count();
                        if (checkdoc == 0)
                        {
                            TrnsEmployeePerPieceRate oDoc = new TrnsEmployeePerPieceRate();
                            dbHrPayroll.TrnsEmployeePerPieceRate.InsertOnSubmit(oDoc);
                            oDoc.MstEmployee = oEmp;
                            oDoc.CreateDt = DateTime.Now;
                            oDoc.UpdateDt = DateTime.Now;
                            oDoc.CreatedBy = oCompany.UserName;
                            oDoc.UpdatedBy = oCompany.UserName;
                            TrnsEmployeePerPieceRateDetail oDetail = new TrnsEmployeePerPieceRateDetail();
                            oDetail.ItemCode = ItemCode;
                            oDetail.ItemName = ItemName;
                            oDetail.Rate = Convert.ToDecimal(ItemRate);
                            oDetail.FlgActive = true;
                            oDetail.FlgDelete = false;
                            oDetail.CreatedBy = oCompany.UserName;
                            oDetail.UpdatedBy = oCompany.UserName;
                            oDetail.CreatedDt = DateTime.Now;
                            oDetail.UpdatedDt = DateTime.Now;
                            oDoc.TrnsEmployeePerPieceRateDetail.Add(oDetail);

                        }
                        else
                        {
                            TrnsEmployeePerPieceRate oDoc = (from a in dbHrPayroll.TrnsEmployeePerPieceRate where a.EmpID == oEmp.ID select a).FirstOrDefault();
                            if (oDoc == null) continue;
                            oDoc.UpdateDt = DateTime.Now;
                            oDoc.UpdatedBy = oCompany.UserName;
                            int linecount = 0;
                            linecount = (from a in dbHrPayroll.TrnsEmployeePerPieceRateDetail where a.ItemCode == ItemCode && a.FKID == oDoc.InternalID && a.FlgActive == true select a).Count();
                            if (linecount == 0)
                            {
                                TrnsEmployeePerPieceRateDetail oDetail = new TrnsEmployeePerPieceRateDetail();
                                oDetail.ItemCode = ItemCode;
                                oDetail.ItemName = ItemName;
                                oDetail.Rate = Convert.ToDecimal(ItemRate);
                                oDetail.FlgActive = true;
                                oDetail.FlgDelete = false;
                                oDetail.CreatedBy = oCompany.UserName;
                                oDetail.UpdatedBy = oCompany.UserName;
                                oDetail.CreatedDt = DateTime.Now;
                                oDetail.UpdatedDt = DateTime.Now;
                                oDoc.TrnsEmployeePerPieceRateDetail.Add(oDetail);
                            }
                            else
                            {
                                TrnsEmployeePerPieceRateDetail oDetail = (from a in dbHrPayroll.TrnsEmployeePerPieceRateDetail where a.ItemCode == ItemCode && a.FKID == oDoc.InternalID && a.FlgActive == true select a).FirstOrDefault();
                                oDetail.Rate = Convert.ToDecimal(ItemRate);
                                oDetail.FlgActive = true;
                                oDetail.FlgDelete = false;
                                oDetail.UpdatedBy = oCompany.UserName;
                                oDetail.UpdatedDt = DateTime.Now;
                            }
                        }
                    }
                    dbHrPayroll.SubmitChanges();
                    oApplication.StatusBar.SetText("Data successfully uploaded.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                    ClearRecords();
                }
                else
                {
                    oApplication.StatusBar.SetText("No data available for import, Please select template.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return;
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("PostEmployeePerPieceRate : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void PostShiftData()
        {
            try
            {
                int ErrorAtLine = 1;
                string EmpCode = "";
                oApplication.StatusBar.SetText("Please wait. Validation of Data started.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                if (dtMat.Rows.Count > 0)
                {
                    for (int i = 0; i < dtMat.Rows.Count; i++)
                    {
                        ErrorAtLine = i + 1;
                        string EmployeeCode, ShiftCode, ShiftDate;
                        string dayofWeeks;
                        DateTime dtShiftDay = DateTime.Now;
                        EmployeeCode = dtMat.GetValue("EmpCode", i);
                        ShiftCode = dtMat.GetValue("ShiftCode", i);
                        ShiftDate = dtMat.GetValue("ShiftDate", i);
                        if (!string.IsNullOrEmpty(EmployeeCode))
                        {
                            EmpCode = EmployeeCode;
                            int check = (from a in dbHrPayroll.MstEmployee where a.EmpID == EmployeeCode select a).Count();
                            if (check == 0)
                            {
                                oApplication.StatusBar.SetText("Employee # " + EmpCode + " not found error at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                continue;
                            }
                        }
                        else
                        {
                            oApplication.StatusBar.SetText("EmpCode is empty. at Line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            continue;
                        }
                        if (!string.IsNullOrEmpty(ShiftCode))
                        {
                            int check = (from a in dbHrPayroll.MstShifts where a.Code == ShiftCode select a).Count();
                            if (check == 0)
                            {
                                oApplication.StatusBar.SetText("Shift Code not found. at Line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                continue;
                            }
                        }
                        else
                        {
                            oApplication.StatusBar.SetText("Shift Code is empty. at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            continue;
                        }
                        if (!string.IsNullOrEmpty(ShiftDate))
                        {
                            try
                            {
                                dtShiftDay = DateTime.ParseExact(ShiftDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                dayofWeeks = Convert.ToString(dtShiftDay.DayOfWeek);
                                int check = (from a in dbHrPayroll.CfgPeriodDates where a.StartDate <= dtShiftDay && a.EndDate >= dtShiftDay select a).Count();
                                if (check == 0)
                                {
                                    oApplication.StatusBar.SetText("Period of define date not found. at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    continue;
                                }
                            }
                            catch
                            {
                                oApplication.StatusBar.SetText("enter valid date format at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                continue;
                            }
                        }
                        else
                        {
                            oApplication.StatusBar.SetText("Shift date is empty. at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            continue;
                        }

                        TrnsAttendanceRegister oDoc = (from a in dbHrPayroll.TrnsAttendanceRegister where a.MstEmployee.EmpID == EmpCode && a.Date == dtShiftDay select a).FirstOrDefault();
                        if (oDoc == null)
                        {
                            var oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == EmpCode select a).FirstOrDefault();
                            var oPeriod = (from a in dbHrPayroll.CfgPeriodDates where a.StartDate <= dtShiftDay && a.EndDate >= dtShiftDay select a).FirstOrDefault();
                            var oShift = (from a in dbHrPayroll.MstShifts where a.Code == ShiftCode select a).FirstOrDefault();
                            TrnsAttendanceRegister oNew = new TrnsAttendanceRegister();
                            oNew.MstEmployee = oEmp;
                            oNew.MstShifts = oShift;
                            oNew.PeriodID = oPeriod.ID;
                            oNew.Date = dtShiftDay;
                            oNew.DateDay = dayofWeeks;
                            oNew.Processed = false;
                            oNew.CreateDate = DateTime.Now;
                            oNew.UpdateDate = DateTime.Now;
                            oNew.UserId = "UL" + oCompany.UserName;
                            oNew.UpdatedBy = "UL" + oCompany.UserName;
                        }
                        else
                        {
                            if (oDoc.FlgPosted == true)
                            {
                                oApplication.StatusBar.SetText("you can't assign shifts of already processed attendance.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            }
                            else
                            {
                                var oPeriod = (from a in dbHrPayroll.CfgPeriodDates where a.StartDate <= dtShiftDay && a.EndDate >= dtShiftDay select a).FirstOrDefault();
                                var oShift = (from a in dbHrPayroll.MstShifts where a.Code == ShiftCode select a).FirstOrDefault();
                                oDoc.PeriodID = oPeriod.ID;
                                oDoc.MstShifts = oShift;
                                oDoc.UpdateDate = DateTime.Now;
                                oDoc.UpdatedBy = "UL" + oCompany.UserName;
                            }
                        }

                    }
                    oApplication.StatusBar.SetText("Please wait Validation is completed system uploading data.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    dbHrPayroll.SubmitChanges();
                    oApplication.StatusBar.SetText("Data successfully uploaded.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                    ClearRecords();
                }
                else
                {
                    oApplication.StatusBar.SetText("No data available for import, Please select template.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return;
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Shift Import failed. " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void PostLeavesData()
        {
            try
            {

                int ErrorAtLine = 1;
                string EmpCode = "";
                oApplication.StatusBar.SetText("Please wait. Validation of Data started.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                if (dtMat.Rows.Count > 0)
                {
                    for (int i = 0; i < dtMat.Rows.Count; i++)
                    {
                        ErrorAtLine = i + 1;
                        string EmployeeCode, PeriodCode, LeaveTypeCode, strLeaveCount;
                        double LeaveCount;
                        DateTime dtShiftDay = DateTime.Now;
                        DateTime dateFrom;
                        DateTime dateTo;
                        EmployeeCode = dtMat.GetValue("EmpCode", i);
                        if (string.IsNullOrWhiteSpace(EmployeeCode))
                        {
                            continue;
                        }
                        //PeriodCode = dtMat.GetValue("PeriodCode", i);
                        LeaveTypeCode = dtMat.GetValue("LeaveTypeCode", i);
                        strLeaveCount = dtMat.GetValue("LeaveCount", i);


                        dateFrom = Convert.ToDateTime(dtMat.GetValue("DateFrom", i));
                        dateTo = Convert.ToDateTime(dtMat.GetValue("DateTo", i));

                        if (!string.IsNullOrEmpty(EmployeeCode))
                        {
                            EmpCode = EmployeeCode;
                            int check = (from a in dbHrPayroll.MstEmployee where a.EmpID == EmployeeCode && a.FlgActive == true select a).Count();
                            if (check == 0)
                            {
                                oApplication.StatusBar.SetText("Employee # " + EmpCode + " not found error at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                continue;
                            }
                        }
                        else
                        {
                            oApplication.StatusBar.SetText("EmpCode is empty. at Line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            continue;
                        }
                        //if (!string.IsNullOrEmpty(PeriodCode))
                        //{
                        //    int check = (from a in dbHrPayroll.CfgPeriodDates where a.PeriodName == PeriodCode select a).Count();
                        //    if (check == 0)
                        //    {
                        //        oApplication.StatusBar.SetText("Period Code not found. at Line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        //        continue;
                        //    }
                        //}
                        //else
                        //{
                        //    oApplication.StatusBar.SetText("Period Code is empty. at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        //    continue;
                        //}
                        if (!string.IsNullOrEmpty(LeaveTypeCode))
                        {
                            int check = (from a in dbHrPayroll.MstLeaveType where a.Code == LeaveTypeCode select a).Count();
                            if (check == 0)
                            {
                                oApplication.StatusBar.SetText("Leave Type Code not found. at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                continue;
                            }
                        }
                        else
                        {
                            oApplication.StatusBar.SetText("Shift date is empty. at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            continue;
                        }
                        if (!string.IsNullOrEmpty(strLeaveCount))
                        {
                            try
                            {
                                LeaveCount = Convert.ToDouble(strLeaveCount);
                                if (LeaveCount > 0)
                                {
                                    MstEmployee oEMP = (from a in dbHrPayroll.MstEmployee where a.EmpID == EmpCode select a).FirstOrDefault();
                                    MstCalendar oCal = (from a in dbHrPayroll.MstCalendar where a.FlgActive == true select a).FirstOrDefault();
                                    MstEmployeeLeaves oEmpLeave = (from a in dbHrPayroll.MstEmployeeLeaves
                                                                   where a.MstEmployee.EmpID == EmpCode
                                                                   && a.MstLeaveType.Code == LeaveTypeCode
                                                                   && a.LeaveCalCode == oCal.Code
                                                                   select a).FirstOrDefault();
                                    if (oEmpLeave == null)
                                    {
                                        oApplication.StatusBar.SetText("Leave Assignment required before entering leaves. at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                        continue;
                                    }
                                    else
                                    {
                                        decimal? LeaveCarryForward = 0, LeaveEntitled = 0, TotalAvailable = 0, LeaveUsed = 0, deductedLeaves, ApprovedLeaves = 0, RequestedLeaves = 0, AllowedAccured = 0;
                                        int intLeaveID = 0;
                                        MstLeaveType oLeaveType = (from a in dbHrPayroll.MstLeaveType where a.Code == LeaveTypeCode select a).FirstOrDefault();
                                        if (oLeaveType != null) intLeaveID = oLeaveType.ID;

                                        var RequestedLeavesRecords = dbHrPayroll.TrnsLeavesRequest.Where(a => a.EmpID == oEMP.ID && a.LeaveType == intLeaveID && a.DocAprStatus == "LV0005" && a.LeaveFrom >= oCal.StartDate && a.LeaveTo <= oCal.EndDate).GroupBy(a => a.EmpID).Select(a => new { Amount = a.Sum(b => b.TotalCount) }).OrderByDescending(a => a.Amount).ToList();
                                        if (RequestedLeavesRecords != null && RequestedLeavesRecords.Count > 0)
                                        {
                                            RequestedLeaves = RequestedLeavesRecords.FirstOrDefault().Amount;
                                        }
                                        var ApprovedLeavesRecords = dbHrPayroll.TrnsLeavesRequest.Where(a => a.EmpID == oEMP.ID && a.LeaveType == intLeaveID && a.DocAprStatus == "LV0006" && a.LeaveFrom >= oCal.StartDate && a.LeaveTo <= oCal.EndDate).GroupBy(a => a.EmpID).Select(a => new { Amount = a.Sum(b => b.TotalCount) }).OrderByDescending(a => a.Amount).ToList();
                                        if (ApprovedLeavesRecords != null && ApprovedLeavesRecords.Count > 0)
                                        {
                                            ApprovedLeaves = ApprovedLeavesRecords.FirstOrDefault().Amount;
                                        }
                                        if (oEmpLeave.LeavesCarryForward != null)
                                        {
                                            LeaveCarryForward = oEmpLeave.LeavesCarryForward;
                                        }
                                        if (oEmpLeave.LeavesEntitled != null)
                                        {
                                            LeaveEntitled = oEmpLeave.LeavesEntitled;
                                        }
                                        if (LeaveCarryForward != null && LeaveEntitled != null)
                                        {
                                            TotalAvailable = LeaveCarryForward + LeaveEntitled;
                                        }
                                        if (oEmpLeave.LeavesUsed != null)
                                        {
                                            LeaveUsed = oEmpLeave.LeavesUsed;
                                        }
                                        deductedLeaves = RequestedLeaves + ApprovedLeaves;
                                        double Balance = Convert.ToDouble(TotalAvailable) - Convert.ToDouble(deductedLeaves);
                                        if (LeaveCount > Balance)
                                        {
                                            oApplication.StatusBar.SetText("Leave Count is higher than available balance. at line " + ErrorAtLine.ToString() + " EmpCode : " + EmployeeCode, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                            continue;
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                LeaveCount = 0;
                                oApplication.StatusBar.SetText("Leave Count should be number. at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            }
                        }
                        else
                        {
                            oApplication.StatusBar.SetText("Leave Count is empty. at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            LeaveCount = 0;
                            continue;
                        }

                        if (LeaveCount > 0)
                        {
                            MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee
                                                where a.EmpID == EmpCode
                                                && a.FlgActive == true
                                                select a).FirstOrDefault();

                            MstLeaveType oLeaveType = (from a in dbHrPayroll.MstLeaveType
                                                       where a.Code == LeaveTypeCode
                                                       && a.Active == true
                                                       select a).FirstOrDefault();

                            DateTime StartDate = Convert.ToDateTime(dateFrom);
                            DateTime EndDate = Convert.ToDateTime(dateTo);
                            double ActiveLeaveCount = 1;
                            int LeaveDays = 0;
                            LeaveDays = Convert.ToInt32((EndDate - StartDate).TotalDays + 1);
                            if (LeaveCount > LeaveDays)
                            {
                                oApplication.StatusBar.SetText("Employee  '" + EmpCode + "'  Leave Count is greater date from and date to. at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                LeaveCount = 0;
                                continue;
                            }
                            else if (LeaveCount < LeaveDays)
                            {
                                oApplication.StatusBar.SetText("Employee  '" + EmpCode + "'  Leave Date from and date to Count is greater then Leave Count. at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                LeaveCount = 0;
                                continue;
                            }
                            //for (DateTime x = StartDate; x <= EndDate && ActiveLeaveCount <= LeaveCount; x = x.AddDays(1))
                            //{
                            int LeaveExists = (from a in dbHrPayroll.TrnsLeavesRequest
                                               where a.MstEmployee.EmpID == EmpCode
                                               && a.LeaveFrom <= StartDate
                                               && a.LeaveTo >= EndDate
                                               select a).Count();

                            if (LeaveExists < 1)
                            {
                                using (dbHRMS oDBPrivate = new dbHRMS(Program.ConStrHRMS))
                                {
                                    int? DocNum = oDBPrivate.TrnsLeavesRequest.Max(a => (int?)a.DocNum);
                                    DocNum = DocNum == null ? 1 : DocNum + 1;
                                    TrnsLeavesRequest LeaveRequest = new TrnsLeavesRequest();
                                    LeaveRequest.DocNum = DocNum;
                                    LeaveRequest.Series = -1;
                                    LeaveRequest.EmpID = oEmp.ID;
                                    LeaveRequest.EmpName = oEmp.FirstName + " " + oEmp.LastName;
                                    LeaveRequest.UnitsID = "Day";
                                    LeaveRequest.UnitsLOVType = "LeaveUnits";
                                    LeaveRequest.LeaveFrom = StartDate;
                                    LeaveRequest.LeaveTo = EndDate;
                                    LeaveRequest.TotalCount = Convert.ToDecimal(LeaveCount);//Convert.ToDecimal(ActiveLeaveCount - (ActiveLeaveCount - 1));

                                    LeaveRequest.Units = Convert.ToInt32(480);
                                    LeaveRequest.LeaveType = oLeaveType.ID;
                                    LeaveRequest.LeaveDescription = oLeaveType.Description;
                                    LeaveRequest.FlgPaid = false;
                                    LeaveRequest.DocDate = DateTime.Now;
                                    LeaveRequest.CreateDate = DateTime.Now;
                                    LeaveRequest.CreatedBy = oCompany.UserName + " DU";
                                    oDBPrivate.TrnsLeavesRequest.InsertOnSubmit(LeaveRequest);
                                    oDBPrivate.SubmitChanges();
                                    ActiveLeaveCount++;
                                }
                            }
                            //}
                        }
                    }
                    //oApplication.StatusBar.SetText("Please wait Validation is completed system uploading data.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    oApplication.StatusBar.SetText("Data successfully uploaded.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                    ClearRecords();

                }
                else
                {
                    oApplication.StatusBar.SetText("No data available for import, Please select template.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return;
                }

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("PostLeavesData : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void DeletePostLeavesData()
        {
            try
            {

                int ErrorAtLine = 1;
                string EmpCode = "";
                oApplication.StatusBar.SetText("Please wait. Validation of Data started.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                if (dtMat.Rows.Count > 0)
                {
                    for (int i = 0; i < dtMat.Rows.Count; i++)
                    {
                        ErrorAtLine = i + 1;
                        string EmployeeCode, PeriodCode, LeaveTypeCode, strLeaveCount;
                        double LeaveCount;
                        DateTime dtShiftDay = DateTime.Now;
                        DateTime dateFrom;
                        DateTime dateTo;
                        EmployeeCode = dtMat.GetValue("EmpCode", i);
                        if (string.IsNullOrWhiteSpace(EmployeeCode))
                        {
                            continue;
                        }
                        LeaveTypeCode = dtMat.GetValue("LeaveTypeCode", i);
                        strLeaveCount = dtMat.GetValue("LeaveCount", i);

                        dateFrom = Convert.ToDateTime(dtMat.GetValue("DateFrom", i));
                        dateTo = Convert.ToDateTime(dtMat.GetValue("DateTo", i));
                        if (!string.IsNullOrEmpty(EmployeeCode))
                        {
                            EmpCode = EmployeeCode;
                            int check = (from a in dbHrPayroll.MstEmployee where a.EmpID == EmployeeCode && a.FlgActive == true select a).Count();
                            if (check == 0)
                            {
                                oApplication.StatusBar.SetText("Employee # " + EmpCode + " not found error at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                continue;
                            }
                        }
                        else
                        {
                            oApplication.StatusBar.SetText("EmpCode is empty. at Line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            continue;
                        }

                        if (!string.IsNullOrEmpty(LeaveTypeCode))
                        {
                            int check = (from a in dbHrPayroll.MstLeaveType where a.Code == LeaveTypeCode select a).Count();
                            if (check == 0)
                            {
                                oApplication.StatusBar.SetText("Leave Type Code not found. at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                continue;
                            }
                        }
                        else
                        {
                            oApplication.StatusBar.SetText("Shift date is empty. at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            continue;
                        }
                        if (!string.IsNullOrEmpty(strLeaveCount))
                        {
                            try
                            {
                                LeaveCount = Convert.ToDouble(strLeaveCount);
                                if (LeaveCount > 0)
                                {
                                    var oEmpLeave = (from a in dbHrPayroll.MstEmployeeLeaves
                                                     where a.MstEmployee.EmpID == EmpCode && a.MstLeaveType.Code == LeaveTypeCode
                                                     select a).FirstOrDefault();
                                    if (oEmpLeave == null)
                                    {
                                        oApplication.StatusBar.SetText("Leave Assignment required before entering leaves. at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                        continue;
                                    }
                                    else
                                    {
                                        decimal? LeaveCarryForward = 0, LeaveEntitled = 0, TotalAvailable = 0, LeaveUsed = 0, deductedLeaves, ApprovedLeaves = 0, RequestedLeaves = 0, AllowedAccured = 0;
                                        int intLeaveID = 0;
                                        MstEmployee oEMP = (from a in dbHrPayroll.MstEmployee where a.EmpID == EmpCode select a).FirstOrDefault();
                                        MstCalendar oCal = (from a in dbHrPayroll.MstCalendar where a.FlgActive == true select a).FirstOrDefault();
                                        MstLeaveType oLeaveType = (from a in dbHrPayroll.MstLeaveType where a.Code == LeaveTypeCode select a).FirstOrDefault();
                                        if (oLeaveType != null) intLeaveID = oLeaveType.ID;

                                        var RequestedLeavesRecords = dbHrPayroll.TrnsLeavesRequest.Where(a => a.EmpID == oEMP.ID && a.LeaveType == intLeaveID && a.DocAprStatus == "LV0005" && a.LeaveFrom >= oCal.StartDate && a.LeaveTo <= oCal.EndDate).GroupBy(a => a.EmpID).Select(a => new { Amount = a.Sum(b => b.TotalCount) }).OrderByDescending(a => a.Amount).ToList();
                                        if (RequestedLeavesRecords != null && RequestedLeavesRecords.Count > 0)
                                        {
                                            RequestedLeaves = RequestedLeavesRecords.FirstOrDefault().Amount;
                                        }
                                        var ApprovedLeavesRecords = dbHrPayroll.TrnsLeavesRequest.Where(a => a.EmpID == oEMP.ID && a.LeaveType == intLeaveID && a.DocAprStatus == "LV0006" && a.LeaveFrom >= oCal.StartDate && a.LeaveTo <= oCal.EndDate).GroupBy(a => a.EmpID).Select(a => new { Amount = a.Sum(b => b.TotalCount) }).OrderByDescending(a => a.Amount).ToList();
                                        if (ApprovedLeavesRecords != null && ApprovedLeavesRecords.Count > 0)
                                        {
                                            ApprovedLeaves = ApprovedLeavesRecords.FirstOrDefault().Amount;
                                        }
                                        if (oEmpLeave.LeavesCarryForward != null)
                                        {
                                            LeaveCarryForward = oEmpLeave.LeavesCarryForward;
                                        }
                                        if (oEmpLeave.LeavesEntitled != null)
                                        {
                                            LeaveEntitled = oEmpLeave.LeavesEntitled;
                                        }
                                        if (LeaveCarryForward != null && LeaveEntitled != null)
                                        {
                                            TotalAvailable = LeaveCarryForward + LeaveEntitled;
                                        }
                                        if (oEmpLeave.LeavesUsed != null)
                                        {
                                            LeaveUsed = oEmpLeave.LeavesUsed;
                                        }
                                        deductedLeaves = RequestedLeaves + ApprovedLeaves;
                                        double Balance = Convert.ToDouble(TotalAvailable) - Convert.ToDouble(deductedLeaves);
                                        if (LeaveCount > Balance)
                                        {
                                            oApplication.StatusBar.SetText("Leave Count is higher than available balance. at line " + ErrorAtLine.ToString() + " EmpCode : " + EmployeeCode, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                            continue;
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                LeaveCount = 0;
                                oApplication.StatusBar.SetText("Leave Count should be number. at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            }
                        }
                        else
                        {
                            oApplication.StatusBar.SetText("Leave Count is empty. at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            LeaveCount = 0;
                            continue;
                        }

                        if (LeaveCount > 0)
                        {
                            MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == EmpCode && a.FlgActive == true select a).FirstOrDefault();
                            MstLeaveType oLeaveType = (from a in dbHrPayroll.MstLeaveType where a.Code == LeaveTypeCode && a.Active == true select a).FirstOrDefault();
                            DateTime StartDate = Convert.ToDateTime(dateFrom);
                            DateTime EndDate = Convert.ToDateTime(dateTo);
                            double ActiveLeaveCount = 1;
                            int LeaveDays = Convert.ToInt32((StartDate - EndDate).TotalDays);
                            for (DateTime x = StartDate; x <= EndDate && ActiveLeaveCount <= LeaveCount; x = x.AddDays(1))
                            {
                                int LeaveExists = (from a in dbHrPayroll.TrnsLeavesRequest
                                                   where a.MstEmployee.EmpID == EmpCode
                                                   && a.LeaveFrom <= x && a.LeaveTo >= x
                                                   select a).Count();

                                if (LeaveExists >= 1)
                                {
                                    var varLeaveExists = (from a in dbHrPayroll.TrnsLeavesRequest
                                                          where a.MstEmployee.EmpID == EmpCode
                                                          && a.LeaveFrom <= x
                                                          && a.LeaveTo >= x
                                                          select a).FirstOrDefault();
                                    TrnsLeavesRequest LeaveDoc = dbHrPayroll.TrnsLeavesRequest.Where(lr => lr.DocNum == Convert.ToInt32(varLeaveExists.DocNum)).FirstOrDefault();
                                    if (LeaveDoc != null)
                                    {
                                        dbHrPayroll.TrnsLeavesRequest.DeleteOnSubmit(LeaveDoc);
                                    }
                                    dbHrPayroll.SubmitChanges();
                                    ActiveLeaveCount++;
                                }
                            }
                        }
                    }
                    //oApplication.StatusBar.SetText("Please wait Validation is completed system uploading data.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    oApplication.StatusBar.SetText("Data successfully uploaded.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                    ClearRecords();

                }
                else
                {
                    oApplication.StatusBar.SetText("No data available for import, Please select template.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return;
                }

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("PostLeavesData : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void PostLeavesDataOld1()
        {
            try
            {
                int ErrorAtLine = 1;
                string EmpCode = "";
                oApplication.StatusBar.SetText("Please wait. Validation of Data started.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                if (dtMat.Rows.Count > 0)
                {
                    for (int i = 0; i < dtMat.Rows.Count; i++)
                    {
                        ErrorAtLine = i + 1;
                        string EmployeeCode, PeriodCode, LeaveTypeCode, strLeaveCount;
                        double LeaveCount;
                        DateTime dtShiftDay = DateTime.Now;
                        DateTime dateFrom;
                        DateTime dateTo;
                        EmployeeCode = dtMat.GetValue("EmpCode", i);
                        if (string.IsNullOrWhiteSpace(EmployeeCode))
                        {
                            continue;
                        }
                        //PeriodCode = dtMat.GetValue("PeriodCode", i);
                        LeaveTypeCode = dtMat.GetValue("LeaveTypeCode", i);
                        strLeaveCount = dtMat.GetValue("LeaveCount", i);


                        dateFrom = Convert.ToDateTime(dtMat.GetValue("DateFrom", i));
                        dateTo = Convert.ToDateTime(dtMat.GetValue("DateTo", i));
                        if (!string.IsNullOrEmpty(EmployeeCode))
                        {
                            EmpCode = EmployeeCode;
                            int check = (from a in dbHrPayroll.MstEmployee where a.EmpID == EmployeeCode && a.FlgActive == true select a).Count();
                            if (check == 0)
                            {
                                oApplication.StatusBar.SetText("Employee # " + EmpCode + " not found error at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                continue;
                            }
                        }
                        else
                        {
                            oApplication.StatusBar.SetText("EmpCode is empty. at Line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            continue;
                        }
                        //if (!string.IsNullOrEmpty(PeriodCode))
                        //{
                        //    int check = (from a in dbHrPayroll.CfgPeriodDates where a.PeriodName == PeriodCode select a).Count();
                        //    if (check == 0)
                        //    {
                        //        oApplication.StatusBar.SetText("Period Code not found. at Line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        //        continue;
                        //    }
                        //}
                        //else
                        //{
                        //    oApplication.StatusBar.SetText("Period Code is empty. at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        //    continue;
                        //}
                        if (!string.IsNullOrEmpty(LeaveTypeCode))
                        {
                            int check = (from a in dbHrPayroll.MstLeaveType where a.Code == LeaveTypeCode select a).Count();
                            if (check == 0)
                            {
                                oApplication.StatusBar.SetText("Leave Type Code not found. at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                continue;
                            }
                        }
                        else
                        {
                            oApplication.StatusBar.SetText("Shift date is empty. at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            continue;
                        }
                        if (!string.IsNullOrEmpty(strLeaveCount))
                        {
                            try
                            {
                                LeaveCount = Convert.ToDouble(strLeaveCount);
                                if (LeaveCount > 0)
                                {
                                    var oEmpLeave = (from a in dbHrPayroll.MstEmployeeLeaves
                                                     where a.MstEmployee.EmpID == EmpCode && a.MstLeaveType.Code == LeaveTypeCode
                                                     select a).FirstOrDefault();
                                    if (oEmpLeave == null)
                                    {
                                        oApplication.StatusBar.SetText("Leave Assignment required before entering leaves. at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                        continue;
                                    }
                                    else
                                    {
                                        decimal? LeaveCarryForward = 0, LeaveEntitled = 0, TotalAvailable = 0, LeaveUsed = 0, deductedLeaves, ApprovedLeaves = 0, RequestedLeaves = 0, AllowedAccured = 0;
                                        int intLeaveID = 0;
                                        MstEmployee oEMP = (from a in dbHrPayroll.MstEmployee where a.EmpID == EmpCode select a).FirstOrDefault();
                                        MstCalendar oCal = (from a in dbHrPayroll.MstCalendar where a.FlgActive == true select a).FirstOrDefault();
                                        MstLeaveType oLeaveType = (from a in dbHrPayroll.MstLeaveType where a.Code == LeaveTypeCode select a).FirstOrDefault();
                                        if (oLeaveType != null) intLeaveID = oLeaveType.ID;

                                        var RequestedLeavesRecords = dbHrPayroll.TrnsLeavesRequest.Where(a => a.EmpID == oEMP.ID && a.LeaveType == intLeaveID && a.DocAprStatus == "LV0005" && a.LeaveFrom >= oCal.StartDate && a.LeaveTo <= oCal.EndDate).GroupBy(a => a.EmpID).Select(a => new { Amount = a.Sum(b => b.TotalCount) }).OrderByDescending(a => a.Amount).ToList();
                                        if (RequestedLeavesRecords != null && RequestedLeavesRecords.Count > 0)
                                        {
                                            RequestedLeaves = RequestedLeavesRecords.FirstOrDefault().Amount;
                                        }
                                        var ApprovedLeavesRecords = dbHrPayroll.TrnsLeavesRequest.Where(a => a.EmpID == oEMP.ID && a.LeaveType == intLeaveID && a.DocAprStatus == "LV0006" && a.LeaveFrom >= oCal.StartDate && a.LeaveTo <= oCal.EndDate).GroupBy(a => a.EmpID).Select(a => new { Amount = a.Sum(b => b.TotalCount) }).OrderByDescending(a => a.Amount).ToList();
                                        if (ApprovedLeavesRecords != null && ApprovedLeavesRecords.Count > 0)
                                        {
                                            ApprovedLeaves = ApprovedLeavesRecords.FirstOrDefault().Amount;
                                        }
                                        if (oEmpLeave.LeavesCarryForward != null)
                                        {
                                            LeaveCarryForward = oEmpLeave.LeavesCarryForward;
                                        }
                                        if (oEmpLeave.LeavesEntitled != null)
                                        {
                                            LeaveEntitled = oEmpLeave.LeavesEntitled;
                                        }
                                        if (LeaveCarryForward != null && LeaveEntitled != null)
                                        {
                                            TotalAvailable = LeaveCarryForward + LeaveEntitled;
                                        }
                                        if (oEmpLeave.LeavesUsed != null)
                                        {
                                            LeaveUsed = oEmpLeave.LeavesUsed;
                                        }
                                        deductedLeaves = RequestedLeaves + ApprovedLeaves;
                                        double Balance = Convert.ToDouble(TotalAvailable) - Convert.ToDouble(deductedLeaves);
                                        if (LeaveCount > Balance)
                                        {
                                            oApplication.StatusBar.SetText("Leave Count is higher than available balance. at line " + ErrorAtLine.ToString() + " EmpCode : " + EmployeeCode, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                            continue;
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                LeaveCount = 0;
                                oApplication.StatusBar.SetText("Leave Count should be number. at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            }
                        }
                        else
                        {
                            oApplication.StatusBar.SetText("Leave Count is empty. at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            LeaveCount = 0;
                            continue;
                        }

                        if (LeaveCount > 0)
                        {
                            //CfgPeriodDates oPeriod = (from a in dbHrPayroll.CfgPeriodDates where a.PeriodName == PeriodCode select a).FirstOrDefault();
                            MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == EmpCode && a.FlgActive == true select a).FirstOrDefault();
                            MstLeaveType oLeaveType = (from a in dbHrPayroll.MstLeaveType where a.Code == LeaveTypeCode && a.Active == true select a).FirstOrDefault();
                            //if (oPeriod == null) continue;
                            //DateTime StartDate = Convert.ToDateTime(oPeriod.StartDate);
                            //DateTime EndDate = Convert.ToDateTime(oPeriod.EndDate);
                            DateTime StartDate = Convert.ToDateTime(dateFrom);
                            DateTime EndDate = Convert.ToDateTime(dateTo);
                            double ActiveLeaveCount = 1;
                            int LeaveDays = Convert.ToInt32((StartDate - EndDate).TotalDays);
                            for (DateTime x = StartDate; x <= EndDate && ActiveLeaveCount <= LeaveCount; x = x.AddDays(1))
                            {
                                int LeaveExists = (from a in dbHrPayroll.TrnsLeavesRequest
                                                   where a.MstEmployee.EmpID == EmpCode && a.LeaveFrom <= x && a.LeaveTo >= x
                                                   select a).Count();

                                if (LeaveExists < 1)
                                {
                                    int? DocNum = dbHrPayroll.TrnsLeavesRequest.Max(a => (int?)a.DocNum);
                                    DocNum = DocNum == null ? 1 : DocNum + 1;
                                    TrnsLeavesRequest LeaveRequest = new TrnsLeavesRequest();
                                    LeaveRequest.DocNum = DocNum;
                                    LeaveRequest.Series = -1;
                                    LeaveRequest.EmpID = oEmp.ID;
                                    LeaveRequest.EmpName = oEmp.FirstName + " " + oEmp.LastName;
                                    LeaveRequest.UnitsID = "Day";
                                    LeaveRequest.UnitsLOVType = "LeaveUnits";
                                    LeaveRequest.LeaveFrom = x;
                                    LeaveRequest.LeaveTo = x;
                                    LeaveRequest.TotalCount = Convert.ToDecimal(ActiveLeaveCount - (ActiveLeaveCount - 1));

                                    LeaveRequest.Units = Convert.ToInt32(480);
                                    LeaveRequest.LeaveType = oLeaveType.ID;
                                    LeaveRequest.LeaveDescription = oLeaveType.Description;
                                    LeaveRequest.FlgPaid = false;
                                    LeaveRequest.DocDate = DateTime.Now;
                                    LeaveRequest.CreateDate = DateTime.Now;
                                    LeaveRequest.CreatedBy = oCompany.UserName + " DU";
                                    dbHrPayroll.TrnsLeavesRequest.InsertOnSubmit(LeaveRequest);
                                    dbHrPayroll.SubmitChanges();
                                    ActiveLeaveCount++;
                                }
                            }
                        }
                    }
                    //oApplication.StatusBar.SetText("Please wait Validation is completed system uploading data.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    oApplication.StatusBar.SetText("Data successfully uploaded.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                    ClearRecords();

                }
                else
                {
                    oApplication.StatusBar.SetText("No data available for import, Please select template.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return;
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("PostLeavesData : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void PostLeavesDataOld()
        {
            try
            {
                int ErrorAtLine = 1;
                string EmpCode = "";
                oApplication.StatusBar.SetText("Please wait. Validation of Data started.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                if (dtMat.Rows.Count > 0)
                {

                    for (int i = 0; i < dtMat.Rows.Count; i++)
                    {
                        ErrorAtLine = i + 1;
                        string EmployeeCode, PeriodCode, LeaveTypeCode, strLeaveCount;
                        double LeaveCount;
                        DateTime dtShiftDay = DateTime.Now;
                        EmployeeCode = dtMat.GetValue("EmpCode", i);
                        PeriodCode = dtMat.GetValue("PeriodCode", i);
                        LeaveTypeCode = dtMat.GetValue("LeaveTypeCode", i);
                        strLeaveCount = dtMat.GetValue("LeaveCount", i);
                        if (!string.IsNullOrEmpty(EmployeeCode))
                        {
                            EmpCode = EmployeeCode;
                            int check = (from a in dbHrPayroll.MstEmployee
                                         where a.EmpID == EmployeeCode
                                         && a.FlgActive == true
                                         select a).Count();
                            if (check == 0)
                            {
                                oApplication.StatusBar.SetText("Employee # " + EmpCode + " not found error at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                continue;
                            }
                        }
                        else
                        {
                            oApplication.StatusBar.SetText("EmpCode is empty. at Line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            continue;
                        }
                        if (!string.IsNullOrEmpty(PeriodCode))
                        {
                            int check = (from a in dbHrPayroll.CfgPeriodDates
                                         where a.PeriodName == PeriodCode
                                         select a).Count();
                            if (check == 0)
                            {
                                oApplication.StatusBar.SetText("Period Code not found. at Line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                continue;
                            }
                        }
                        else
                        {
                            oApplication.StatusBar.SetText("Period Code is empty. at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            continue;
                        }
                        if (!string.IsNullOrEmpty(LeaveTypeCode))
                        {
                            int check = (from a in dbHrPayroll.MstLeaveType
                                         where a.Code == LeaveTypeCode
                                         select a).Count();
                            if (check == 0)
                            {
                                oApplication.StatusBar.SetText("Leave Type Code not found. at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                continue;
                            }
                        }
                        else
                        {
                            oApplication.StatusBar.SetText("Shift date is empty. at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            continue;
                        }
                        if (!string.IsNullOrEmpty(strLeaveCount))
                        {
                            try
                            {
                                LeaveCount = Convert.ToDouble(strLeaveCount);
                                if (LeaveCount > 0)
                                {
                                    MstCalendar oCal = (from a in dbHrPayroll.MstCalendar
                                                        where a.FlgActive == true
                                                        select a).FirstOrDefault();

                                    var oEmpLeave = (from a in dbHrPayroll.MstEmployeeLeaves
                                                     where a.MstEmployee.EmpID == EmpCode
                                                     && a.MstLeaveType.Code == LeaveTypeCode
                                                     && a.FromDt >= oCal.StartDate
                                                     && a.ToDt <= oCal.EndDate
                                                     select a).FirstOrDefault();

                                    if (oEmpLeave == null)
                                    {
                                        oApplication.StatusBar.SetText("Leave Assignment required before entering leaves. at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                        continue;
                                    }
                                    else
                                    {
                                        decimal? LeaveCarryForward = 0, LeaveEntitled = 0, TotalAvailable = 0, LeaveUsed = 0, deductedLeaves, ApprovedLeaves = 0, RequestedLeaves = 0, AllowedAccured = 0;
                                        int intLeaveID = 0;
                                        MstEmployee oEMP = (from a in dbHrPayroll.MstEmployee
                                                            where a.EmpID == EmpCode
                                                            select a).FirstOrDefault();
                                        //MstCalendar oCal = (from a in dbHrPayroll.MstCalendar where a.FlgActive == true select a).FirstOrDefault();
                                        MstLeaveType oLeaveType = (from a in dbHrPayroll.MstLeaveType
                                                                   where a.Code == LeaveTypeCode
                                                                   select a).FirstOrDefault();
                                        if (oLeaveType != null) intLeaveID = oLeaveType.ID;

                                        var RequestedLeavesRecords = dbHrPayroll.TrnsLeavesRequest.Where(a => a.EmpID == oEMP.ID && a.LeaveType == intLeaveID && a.DocAprStatus == "LV0005" && a.LeaveFrom >= oCal.StartDate && a.LeaveTo <= oCal.EndDate).GroupBy(a => a.EmpID).Select(a => new { Amount = a.Sum(b => b.TotalCount) }).OrderByDescending(a => a.Amount).ToList();
                                        if (RequestedLeavesRecords != null && RequestedLeavesRecords.Count > 0)
                                        {
                                            RequestedLeaves = RequestedLeavesRecords.FirstOrDefault().Amount;
                                        }
                                        var ApprovedLeavesRecords = dbHrPayroll.TrnsLeavesRequest.Where(a => a.EmpID == oEMP.ID && a.LeaveType == intLeaveID && a.DocAprStatus == "LV0006" && a.LeaveFrom >= oCal.StartDate && a.LeaveTo <= oCal.EndDate).GroupBy(a => a.EmpID).Select(a => new { Amount = a.Sum(b => b.TotalCount) }).OrderByDescending(a => a.Amount).ToList();
                                        if (ApprovedLeavesRecords != null && ApprovedLeavesRecords.Count > 0)
                                        {
                                            ApprovedLeaves = ApprovedLeavesRecords.FirstOrDefault().Amount;
                                        }
                                        if (oEmpLeave.LeavesCarryForward != null)
                                        {
                                            LeaveCarryForward = oEmpLeave.LeavesCarryForward;
                                        }
                                        if (oEmpLeave.LeavesEntitled != null)
                                        {
                                            LeaveEntitled = oEmpLeave.LeavesEntitled;
                                        }
                                        if (LeaveCarryForward != null && LeaveEntitled != null)
                                        {
                                            TotalAvailable = LeaveCarryForward + LeaveEntitled;
                                        }
                                        if (oEmpLeave.LeavesUsed != null)
                                        {
                                            LeaveUsed = oEmpLeave.LeavesUsed;
                                        }
                                        deductedLeaves = RequestedLeaves + ApprovedLeaves;
                                        double Balance = Convert.ToDouble(TotalAvailable) - Convert.ToDouble(deductedLeaves);
                                        if (LeaveCount > Balance)
                                        {
                                            oApplication.StatusBar.SetText("Leave Count is higher than available balance. at line " + ErrorAtLine.ToString() + " EmpCode : " + EmployeeCode, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                            continue;
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                LeaveCount = 0;
                                oApplication.StatusBar.SetText("Leave Count should be number. at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            }
                        }
                        else
                        {
                            oApplication.StatusBar.SetText("Leave Count is empty. at line " + ErrorAtLine.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            LeaveCount = 0;
                            continue;
                        }

                        if (LeaveCount > 0)
                        {
                            CfgPeriodDates oPeriod = (from a in dbHrPayroll.CfgPeriodDates where a.PeriodName == PeriodCode select a).FirstOrDefault();
                            MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == EmpCode && a.FlgActive == true select a).FirstOrDefault();
                            MstLeaveType oLeaveType = (from a in dbHrPayroll.MstLeaveType where a.Code == LeaveTypeCode && a.Active == true select a).FirstOrDefault();
                            if (oPeriod == null) continue;
                            DateTime StartDate = Convert.ToDateTime(oPeriod.StartDate);
                            DateTime EndDate = Convert.ToDateTime(oPeriod.EndDate);
                            double ActiveLeaveCount = 1;
                            for (DateTime x = StartDate; x <= EndDate && ActiveLeaveCount <= LeaveCount; x = x.AddDays(1))
                            {
                                int LeaveExists = (from a in dbHrPayroll.TrnsLeavesRequest
                                                   where a.MstEmployee.EmpID == EmpCode && a.LeaveFrom <= x && a.LeaveTo >= x
                                                   select a).Count();
                                if (LeaveExists < 1)
                                {
                                    int? DocNum = dbHrPayroll.TrnsLeavesRequest.Max(a => (int?)a.DocNum);
                                    DocNum = DocNum == null ? 1 : DocNum + 1;
                                    TrnsLeavesRequest LeaveRequest = new TrnsLeavesRequest();
                                    LeaveRequest.DocNum = DocNum;
                                    LeaveRequest.Series = -1;
                                    LeaveRequest.EmpID = oEmp.ID;
                                    LeaveRequest.EmpName = oEmp.FirstName + " " + oEmp.LastName;
                                    LeaveRequest.UnitsID = "Day";
                                    LeaveRequest.UnitsLOVType = "LeaveUnits";
                                    LeaveRequest.LeaveFrom = x;
                                    LeaveRequest.LeaveTo = x;
                                    LeaveRequest.TotalCount = Convert.ToDecimal(ActiveLeaveCount - (ActiveLeaveCount - 1));

                                    LeaveRequest.Units = Convert.ToInt32(480);
                                    LeaveRequest.LeaveType = oLeaveType.ID;
                                    LeaveRequest.LeaveDescription = oLeaveType.Description;
                                    LeaveRequest.FlgPaid = false;
                                    LeaveRequest.DocDate = DateTime.Now;
                                    LeaveRequest.CreateDate = DateTime.Now;
                                    LeaveRequest.CreatedBy = oCompany.UserName + " DU";
                                    dbHrPayroll.TrnsLeavesRequest.InsertOnSubmit(LeaveRequest);
                                    dbHrPayroll.SubmitChanges();
                                    ActiveLeaveCount++;
                                }
                            }
                        }
                    }
                    //oApplication.StatusBar.SetText("Please wait Validation is completed system uploading data.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    oApplication.StatusBar.SetText("Data successfully uploaded.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                    ClearRecords();

                }
                else
                {
                    oApplication.StatusBar.SetText("No data available for import, Please select template.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return;
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("PostLeavesData : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void PostRelativeData()
        {
            try
            {
                int ErrorAtLine = 1;
                string EmpCode = "";
                MsgWarning("Please wait. Validation of Data started.");
                if (dtMat.Rows.Count > 0)
                {
                    for (int i = 0; i < dtMat.Rows.Count; i++)
                    {
                        ErrorAtLine = i + 1;
                        string EmployeeCode, Relative, TelephoneNumber, Email, dateofbirth, medicalcard, mcstartdt, mcenddt, dependent;
                        string firstname, lastname, relativecnic;
                        DateTime DOBdt = DateTime.Now;
                        DateTime MCStartdt = DateTime.Now;
                        DateTime MCEnddt = DateTime.Now;
                        EmployeeCode = dtMat.GetValue("EmpCode", i);
                        Relative = dtMat.GetValue("Relatives", i);
                        firstname = dtMat.GetValue("FirstName", i);
                        lastname = dtMat.GetValue("LastName", i);
                        TelephoneNumber = dtMat.GetValue("Telephone", i);
                        Email = dtMat.GetValue("Email", i);
                        dateofbirth = dtMat.GetValue("DOB", i);
                        medicalcard = dtMat.GetValue("MCNumber", i);
                        mcstartdt = dtMat.GetValue("McStartDt", i);
                        mcenddt = dtMat.GetValue("McEndDt", i);
                        dependent = dtMat.GetValue("Dependent", i);
                        relativecnic = dtMat.GetValue("RelativeCNIC", i);
                        if (!string.IsNullOrEmpty(EmployeeCode))
                        {
                            EmpCode = EmployeeCode;
                            int check = (from a in dbHrPayroll.MstEmployee where a.EmpID == EmployeeCode && a.FlgActive == true select a).Count();
                            if (check == 0)
                            {
                                MsgWarning("Employee # " + EmpCode + " not found error at line " + ErrorAtLine.ToString());
                                continue;
                            }
                        }
                        else
                        {
                            MsgWarning("EmpCode is empty. at Line " + ErrorAtLine.ToString());
                            continue;
                        }
                        if (!string.IsNullOrEmpty(Relative))
                        {
                            int check = (from a in dbHrPayroll.MstRelation where a.Code == Relative.Trim() select a).Count();
                            if (check == 0)
                            {
                                MsgWarning("Relative Code not found. at Line " + ErrorAtLine.ToString());
                                continue;
                            }
                        }
                        else
                        {
                            MsgWarning("Relative Code is empty. at line " + ErrorAtLine.ToString());
                            continue;
                        }
                        if (string.IsNullOrEmpty(firstname))
                        {
                            MsgWarning("First name of relative is mandatory.");
                        }
                        if (string.IsNullOrEmpty(lastname))
                        {
                            MsgWarning("Last name of relative is mandatory.");
                        }

                        MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == EmpCode && a.FlgActive == true select a).FirstOrDefault();
                        MstRelation oRelation = (from a in dbHrPayroll.MstRelation where a.Code == Relative.Trim() select a).FirstOrDefault();
                        if (oRelation == null) continue;
                        if (oEmp == null) continue;

                        var oCheck = (from a in dbHrPayroll.MstEmployeeRelatives
                                      where a.EmpID == oEmp.ID
                                      && a.FirstName == firstname.Trim()
                                      select a).Count();
                        MstEmployeeRelatives oDoc;
                        if (oCheck == 0)
                        {
                            oDoc = new MstEmployeeRelatives();
                            dbHrPayroll.MstEmployeeRelatives.InsertOnSubmit(oDoc);
                            oDoc.EmpID = oEmp.ID;
                            oDoc.RelativeID = Convert.ToString(oRelation.Id);
                            oDoc.RelativeLOVType = "MstRelation";
                            oDoc.FirstName = string.IsNullOrEmpty(firstname) ? "" : firstname.Trim();
                            oDoc.CreateDate = DateTime.Now;
                            oDoc.UserId = oCompany.UserName;
                        }
                        else
                        {
                            oDoc = (from a in dbHrPayroll.MstEmployeeRelatives
                                    where a.EmpID == oEmp.ID
                                    && a.FirstName == firstname.Trim()
                                    select a).FirstOrDefault();
                        }

                        oDoc.LastName = string.IsNullOrEmpty(lastname) ? "" : lastname.Trim();
                        oDoc.TelephoneNo = string.IsNullOrEmpty(TelephoneNumber) ? "" : TelephoneNumber;
                        oDoc.Email = string.IsNullOrEmpty(Email) ? "" : Email;
                        //if (dtMat.GetValue("DOB", i) != "") employee.DOB = DateTime.ParseExact(dtMat.GetValue("DOB", i), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        oDoc.BOD = !string.IsNullOrEmpty(dateofbirth) ? DateTime.ParseExact(dateofbirth, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None) : DateTime.Now;
                        oDoc.FlgDependent = dependent.Trim().ToUpper() == "Y" ? true : false;
                        oDoc.IDNoRelative = string.IsNullOrEmpty(relativecnic) ? "" : relativecnic.Trim();
                        oDoc.MedicalCardNo = string.IsNullOrEmpty(medicalcard) ? "" : medicalcard;
                        oDoc.MedicalCardStartDate = !string.IsNullOrEmpty(mcstartdt) ? DateTime.ParseExact(mcstartdt, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None) : DateTime.Now;
                        oDoc.MedicalCardExpiryDate = !string.IsNullOrEmpty(mcenddt) ? DateTime.ParseExact(mcenddt, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None) : DateTime.Now;

                        oDoc.UpdateBy = oCompany.UserName;
                        oDoc.UpdateDate = DateTime.Now;
                    }
                    dbHrPayroll.SubmitChanges();
                    MsgSuccess("Data successfully uploaded.");
                    ClearRecords();
                }
                else
                {
                    oApplication.StatusBar.SetText("No data available for import, Please select template.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return;
                }
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private void PostExperianceData()
        {
            try
            {
                int ErrorAtLine = 1;
                string EmpCode = "";
                MsgWarning("Please wait. Validation of Data started.");
                if (dtMat.Rows.Count > 0)
                {
                    for (int i = 0; i < dtMat.Rows.Count; i++)
                    {
                        ErrorAtLine = i + 1;
                        string EmployeeCode, Company, FromDate, ToDate, Position, Duties, Notes, LastSalary;
                        EmployeeCode = dtMat.GetValue("EmpCode", i);
                        Company = dtMat.GetValue("Company", i);
                        FromDate = dtMat.GetValue("FromDate", i);
                        ToDate = dtMat.GetValue("ToDate", i);
                        Position = dtMat.GetValue("Position", i);
                        Duties = dtMat.GetValue("Durties", i);
                        Notes = dtMat.GetValue("Notes", i);
                        LastSalary = dtMat.GetValue("LastSalary", i);

                        if (!string.IsNullOrEmpty(EmployeeCode))
                        {
                            EmpCode = EmployeeCode;
                            int check = (from a in dbHrPayroll.MstEmployee where a.EmpID == EmployeeCode && a.FlgActive == true select a).Count();
                            if (check == 0)
                            {
                                MsgWarning("Employee # " + EmpCode + " not found error at line " + ErrorAtLine.ToString());
                                continue;
                            }
                        }
                        else
                        {
                            MsgWarning("EmpCode is empty. at Line " + ErrorAtLine.ToString());
                            continue;
                        }
                        if (!string.IsNullOrEmpty(Company))
                        {
                            int check = (from a in dbHrPayroll.MstRelation where a.Code == Company.Trim() select a).Count();
                            if (check == 0)
                            {
                                MsgWarning("Company not found. at Line " + ErrorAtLine.ToString());
                                continue;
                            }
                        }
                        else
                        {
                            MsgWarning("Company field is empty. at line " + ErrorAtLine.ToString());
                            continue;
                        }

                        MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == EmpCode && a.FlgActive == true select a).FirstOrDefault();
                        if (oEmp == null) continue;

                        var oCheck = (from a in dbHrPayroll.MstEmployeeExperience
                                      where a.EmpID == oEmp.ID
                                      && a.CompanyName == Company.Trim()
                                      select a).Count();

                        MstEmployeeExperience oDoc;

                        if (oCheck == 0)
                        {
                            oDoc = new MstEmployeeExperience();
                            dbHrPayroll.MstEmployeeExperience.InsertOnSubmit(oDoc);
                            oDoc.EmpID = oEmp.ID;
                            oDoc.CompanyName = Company;
                            oDoc.CreateDate = DateTime.Now;
                            oDoc.UserId = oCompany.UserName;
                        }
                        else
                        {
                            oDoc = (from a in dbHrPayroll.MstEmployeeExperience
                                    where a.EmpID == oEmp.ID
                                    && a.CompanyName == Company.Trim()
                                    select a).FirstOrDefault();
                        }
                        oDoc.FromDate = !string.IsNullOrEmpty(FromDate) ? DateTime.ParseExact(FromDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None) : DateTime.Now;
                        oDoc.ToDate = !string.IsNullOrEmpty(ToDate) ? DateTime.ParseExact(ToDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None) : DateTime.Now;
                        oDoc.Position = string.IsNullOrEmpty(Position) ? "" : Position.Trim();
                        oDoc.Duties = string.IsNullOrEmpty(Duties) ? "" : Duties.Trim();
                        oDoc.Notes = string.IsNullOrEmpty(Notes) ? "" : Notes.Trim();
                        oDoc.LastSalary = string.IsNullOrEmpty(LastSalary) ? "" : LastSalary.Trim();
                        oDoc.UpdateBy = oCompany.UserName;
                        oDoc.UpdateDate = DateTime.Now;
                    }
                    dbHrPayroll.SubmitChanges();
                    MsgSuccess("Data successfully uploaded.");
                    ClearRecords();
                }
                else
                {
                    MsgWarning("No data available for import, Please select template.");
                    return;
                }
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private void PostCertificationData()
        {
            try
            {
                int ErrorAtLine = 1;
                string EmpCode = "";
                MsgWarning("Please wait. Validation of Data started.");
                if (dtMat.Rows.Count > 0)
                {
                    for (int i = 0; i < dtMat.Rows.Count; i++)
                    {
                        ErrorAtLine = i + 1;
                        string EmployeeCode, Certificates, AwardedBy, AwardedStatus, Description, Notes, Validated;
                        EmployeeCode = dtMat.GetValue("EmpCode", i);
                        Certificates = dtMat.GetValue("CertificationCode", i);
                        AwardedBy = dtMat.GetValue("AwardedBy", i);
                        AwardedStatus = dtMat.GetValue("AwardedStatus", i);
                        Description = dtMat.GetValue("Description", i);
                        Notes = dtMat.GetValue("Notes", i);
                        Validated = dtMat.GetValue("Validated", i);

                        if (!string.IsNullOrEmpty(EmployeeCode))
                        {
                            EmpCode = EmployeeCode;
                            int check = (from a in dbHrPayroll.MstEmployee where a.EmpID == EmployeeCode && a.FlgActive == true select a).Count();
                            if (check == 0)
                            {
                                MsgWarning("Employee # " + EmpCode + " not found error at line " + ErrorAtLine.ToString());
                                continue;
                            }
                        }
                        else
                        {
                            MsgWarning("EmpCode is empty. at Line " + ErrorAtLine.ToString());
                            continue;
                        }
                        if (!string.IsNullOrEmpty(Certificates))
                        {
                            int check = (from a in dbHrPayroll.MstCertification where a.Name == Certificates.Trim() select a).Count();
                            if (check == 0)
                            {
                                MsgWarning("Certificates not found. at Line " + ErrorAtLine.ToString());
                                continue;
                            }
                        }
                        else
                        {
                            MsgWarning("Certificates field is empty. at line " + ErrorAtLine.ToString());
                            continue;
                        }

                        MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == EmpCode && a.FlgActive == true select a).FirstOrDefault();
                        MstCertification oCertificate = (from a in dbHrPayroll.MstCertification where a.Name == Certificates.Trim() select a).FirstOrDefault();
                        if (oEmp == null) continue;
                        if (oCertificate == null) continue;

                        var oCheck = (from a in dbHrPayroll.MstEmployeeCertifications
                                      where a.EmpID == oEmp.ID
                                      && a.CertificationID == oCertificate.Id
                                      select a).Count();

                        MstEmployeeCertifications oDoc;

                        if (oCheck == 0)
                        {
                            oDoc = new MstEmployeeCertifications();
                            dbHrPayroll.MstEmployeeCertifications.InsertOnSubmit(oDoc);
                            oDoc.EmpID = oEmp.ID;
                            oDoc.CertificationID = oCertificate.Id;
                            oDoc.CertificationName = oCertificate.Description;
                            oDoc.CreateDate = DateTime.Now;
                            oDoc.UserId = oCompany.UserName;
                        }
                        else
                        {
                            oDoc = (from a in dbHrPayroll.MstEmployeeCertifications
                                    where a.EmpID == oEmp.ID
                                    && a.CertificationID == oCertificate.Id
                                    select a).FirstOrDefault();
                        }
                        oDoc.AwardedBy = string.IsNullOrEmpty(AwardedBy) ? "" : AwardedBy.Trim();
                        oDoc.AwardStatus = string.IsNullOrEmpty(AwardedStatus) ? "" : AwardedStatus.Trim();
                        oDoc.Description = string.IsNullOrEmpty(Description) ? "" : Description.Trim();
                        oDoc.Notes = string.IsNullOrEmpty(Notes) ? "" : Notes.Trim();
                        oDoc.Validated = string.IsNullOrEmpty(Validated) ? "" : Validated.Trim();
                        oDoc.UpdateDate = DateTime.Now;
                    }
                    dbHrPayroll.SubmitChanges();
                    MsgSuccess("Data successfully uploaded.");
                    ClearRecords();
                }
                else
                {
                    MsgWarning("No data available for import, Please select template.");
                    return;
                }
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private void PostEducationData()
        {
            try
            {
                int ErrorAtLine = 1;
                string EmpCode = "";
                MsgWarning("Please wait. Validation of Data started.");
                if (dtMat.Rows.Count > 0)
                {
                    for (int i = 0; i < dtMat.Rows.Count; i++)
                    {
                        ErrorAtLine = i + 1;
                        string EmployeeCode, InstituteCode, FromDate, ToDate, Subject, Notes, QualificationCode, AwardedQualification, MarkGrade;
                        EmployeeCode = dtMat.GetValue("EmpCode", i);
                        InstituteCode = dtMat.GetValue("InstituteCode", i);
                        FromDate = dtMat.GetValue("FromDate", i);
                        ToDate = dtMat.GetValue("ToDate", i);
                        Subject = dtMat.GetValue("Subject", i);
                        Notes = dtMat.GetValue("Notes", i);
                        QualificationCode = dtMat.GetValue("QualificationCode", i);
                        AwardedQualification = dtMat.GetValue("AwardedQualification", i);
                        MarkGrade = dtMat.GetValue("MarkGrade", i);

                        if (!string.IsNullOrEmpty(EmployeeCode))
                        {
                            EmpCode = EmployeeCode;
                            int check = (from a in dbHrPayroll.MstEmployee where a.EmpID == EmployeeCode && a.FlgActive == true select a).Count();
                            if (check == 0)
                            {
                                MsgWarning("Employee # " + EmpCode + " not found error at line " + ErrorAtLine.ToString());
                                continue;
                            }
                        }
                        else
                        {
                            MsgWarning("EmpCode is empty. at Line " + ErrorAtLine.ToString());
                            continue;
                        }
                        if (!string.IsNullOrEmpty(InstituteCode))
                        {
                            int check = (from a in dbHrPayroll.MstInstitute where a.Code == InstituteCode.Trim() select a).Count();
                            if (check == 0)
                            {
                                MsgWarning("Institute  not found. at Line " + ErrorAtLine.ToString());
                                continue;
                            }
                        }
                        else
                        {
                            MsgWarning("Institute field is empty. at line " + ErrorAtLine.ToString());
                            continue;
                        }
                        if (!string.IsNullOrEmpty(QualificationCode))
                        {
                            int check = (from a in dbHrPayroll.MstQualification where a.Code == QualificationCode.Trim() select a).Count();
                            if (check == 0)
                            {
                                MsgWarning("Qualification  not found. at Line " + ErrorAtLine.ToString());
                                continue;
                            }
                        }
                        else
                        {
                            MsgWarning("Qualification field is empty. at line " + ErrorAtLine.ToString());
                            continue;
                        }

                        MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == EmpCode && a.FlgActive == true select a).FirstOrDefault();
                        MstInstitute oInstitute = (from a in dbHrPayroll.MstInstitute where a.Code == InstituteCode.Trim() select a).FirstOrDefault();
                        MstQualification oQualification = (from a in dbHrPayroll.MstQualification where a.Code == QualificationCode.Trim() select a).FirstOrDefault();
                        if (oEmp == null) continue;
                        if (oInstitute == null) continue;
                        if (oQualification == null) continue;

                        var oCheck = (from a in dbHrPayroll.MstEmployeeEducation
                                      where a.EmpID == oEmp.ID
                                      && a.InstituteID == oInstitute.Id
                                      && a.QualificationID == oQualification.Id
                                      select a).Count();

                        MstEmployeeEducation oDoc;

                        if (oCheck == 0)
                        {
                            oDoc = new MstEmployeeEducation();
                            dbHrPayroll.MstEmployeeEducation.InsertOnSubmit(oDoc);
                            oDoc.EmpID = oEmp.ID;
                            oDoc.InstituteID = oInstitute.Id;
                            oDoc.InstituteName = oInstitute.Name;
                            oDoc.QualificationID = oQualification.Id;
                            oDoc.QualificationName = oQualification.Name;
                            oDoc.CreateDate = DateTime.Now;
                            oDoc.UserId = oCompany.UserName;
                        }
                        else
                        {
                            oDoc = (from a in dbHrPayroll.MstEmployeeEducation
                                    where a.EmpID == oEmp.ID
                                    && a.InstituteID == oInstitute.Id
                                    && a.QualificationID == oQualification.Id
                                    select a).FirstOrDefault();
                        }
                        oDoc.FromDate = !string.IsNullOrEmpty(FromDate) ? DateTime.ParseExact(FromDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None) : DateTime.Now;
                        oDoc.ToDate = !string.IsNullOrEmpty(ToDate) ? DateTime.ParseExact(ToDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None) : DateTime.Now;
                        oDoc.Subject = string.IsNullOrEmpty(Subject) ? "" : Subject.Trim();
                        oDoc.AwardedQualification = string.IsNullOrEmpty(AwardedQualification) ? "" : AwardedQualification.Trim();
                        oDoc.MarkGrade = string.IsNullOrEmpty(MarkGrade) ? "" : MarkGrade.Trim();
                        oDoc.Notes = string.IsNullOrEmpty(Notes) ? "" : Notes.Trim();

                        oDoc.UpdateDate = DateTime.Now;
                    }
                    dbHrPayroll.SubmitChanges();
                    MsgSuccess("Data successfully uploaded.");
                    ClearRecords();
                }
                else
                {
                    MsgWarning("No data available for import, Please select template.");
                    return;
                }
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private bool ValueFoundinMstLOV(string strCode)
        {
            bool recordFound = false;
            try
            {
                var objRecord = dbHrPayroll.MstLOVE.Where(r => r.Code == strCode).FirstOrDefault();
                if (objRecord != null)
                {
                    recordFound = true;
                }
                return recordFound;
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("InitializeFrom Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return false;
            }
        }

        private void ClearRecords()
        {
            txtFileName.Value = "";
            dtMat.Rows.Clear();
            grdDisplay.Clear();

        }

        #endregion

    }
}
