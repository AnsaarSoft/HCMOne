using System;
using System.IO;
using System.Data;
using System.Linq;
using DIHRMS;

using System.Collections;
using System.Collections.Generic;
using DIHRMS.Custom;


namespace ACHR.Screen
{
    partial class frm_DT : HRMSBaseForm
    {
        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);

            InitiallizeForm();
            fillObj();
        }
        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "btPull":
                    pullData();
                    break;
                case "btImport":
                    ImportDataInHRMS();
                    break;
                case "btPick":
                    getFileName();
                    break;

            }
        }
        private void getFileName()
        {
            string fileName = Program.objHrmsUI.FindFile();
            txFilenam.Value = fileName;
        }
        private void pullData()
        {
            pullDataFromFile();
            //if (optOlddb.Selected)
            //{
            //    pullOldPayrollData();
            //}
            //else
            //{
            //    pullDataFromFile();
            //}
        }
        private void pullOldPayrollData()
        {
            switch (cbImpObj.Value.Trim())
            {
                case "1":
                    pullOldDepartments();
                    break;
            }
        }
        private void pullDataFromFile()
        {
            switch (cbImpObj.Value.Trim())
            {
                case "1":
                    pullDeptFromFile();
                    break;
                case "2":
                    pullLocFromFile();
                    break;
                case "3":
                    pullBrancheFromFile();
                    break;
                case "4":
                    pullDesignationFromFile();
                    break;
                case "5":
                    pullPositionFromFile();
                    break;
                case "6":
                    pullEmployeeFromFile();
                    break;
            }

        }
        private void pullOldDepartments()
        {
            sqlString.dbName = txDb.Value.Trim();
            string strSql = sqlString.getImportSql("Department");
            DataTable dt = ds.getDataTable(strSql);
            PopulateGrid(dt);
        }
        private void pullDeptFromFile()
        {

            string fileName = txFilenam.Value.Trim();
            if (fileName == "") return;
            DataTable dt = new DataTable();
            fillDtFromTemplate(dt);
            PopulateGrid(dt);


        }
        private void pullLocFromFile()
        {

            string fileName = txFilenam.Value.Trim();
            if (fileName == "") return;
            DataTable dt = new DataTable();
            fillDtFromTemplate(dt);
            PopulateGrid(dt);


        }
        private void pullBrancheFromFile()
        {

            string fileName = txFilenam.Value.Trim();
            if (fileName == "") return;
            DataTable dt = new DataTable();
            fillDtFromTemplate(dt);
            PopulateGrid(dt);


        }
        private void pullDesignationFromFile()
        {

            string fileName = txFilenam.Value.Trim();
            if (fileName == "") return;
            DataTable dt = new DataTable();
            fillDtFromTemplate(dt);
            PopulateGrid(dt);


        }
        private void pullPositionFromFile()
        {

            string fileName = txFilenam.Value.Trim();
            if (fileName == "") return;
            DataTable dt = new DataTable();
            fillDtFromTemplate(dt);
            PopulateGrid(dt);


        }
        private void pullEmployeeFromFile()
        {

            string fileName = txFilenam.Value.Trim();
            if (fileName == "") return;
            DataTable dt = new DataTable();
            fillDtFromTemplate(dt);
            PopulateGrid(dt);
        }

        private void fillDtFromTemplate(DataTable dt)
        {
            try
            {
                string fileName = txFilenam.Value.Trim();
                if (fileName == "")
                {
                    oApplication.SetStatusBarMessage("Select a template file");
                }

                using (StreamReader file = new StreamReader(fileName))
                {
                    string line = "";
                    string[] pastrts;
                    string strTemplateName = file.ReadLine();
                    if (strTemplateName == null || !strTemplateName.Contains("HRMS Template"))
                    {
                        oApplication.SetStatusBarMessage("Incorrect Template File");
                        return;
                    }
                    line = file.ReadLine();
                    if (line == null)
                    {
                        oApplication.SetStatusBarMessage("Incorrect Template File");
                        return;
                    }
                    pastrts = line.Split('\t');
                    foreach (string colName in pastrts)
                    {
                        dt.Columns.Add(colName);
                    }
                    while ("a" == "a")
                    {
                        line = file.ReadLine();
                        if (line == null) break;
                        pastrts = line.Split('\t');
                        dt.Rows.Add(pastrts);
                        // dt.Rows.Add(pastrts(0), pastrts(1), pastrts(2), pastrts(3), pastrts(4), pastrts(5), pastrts(6), pastrts(7), pastrts(8), pastrts(9), pastrts(10), pastrts(11))
                    }
                }
            }
            catch (Exception ex)
            {
                 oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void PopulateGrid(DataTable dt)
        {
            
                dtMat.Clear();
                SAPbouiCOM.Item oItem;
                SAPbouiCOM.Columns oColumns;
                SAPbouiCOM.DataColumns dtCols;
                dtCols = dtMat.Columns;
                //IbtPrg.Width = 0;

                int maxWidt = ImtObj.Width;
                int rcnt = dt.Rows.Count;
                SAPbouiCOM.ProgressBar prg = oApplication.StatusBar.CreateProgressBar("Loading Data", 17, true);
            try
            {

                int factor = Convert.ToInt16(maxWidt / dt.Rows.Count);

                SAPbouiCOM.Column oColumn;
                SAPbouiCOM.DataColumn dtCol;
                oColumns = mtObj.Columns;
                mtObj.Clear();               
                int i = 0;
                int j = 0;
                try
                {
                    mtObj.LoadFromDataSource();
                    int r = oColumns.Count - 1;
                    for (int m = 0; m < r; m++)
                    {
                        oColumns.Remove("v_" + m.ToString());
                    }
                }
                catch { }
                oForm.Freeze(true);
                foreach (System.Data.DataColumn cl in dt.Columns)
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

                foreach (DataRow dr in dt.Rows)
                {

                    dtMat.Rows.Add(1);
                    j = 0;
                    foreach (System.Data.DataColumn col in dt.Columns)
                    {
                        dtMat.SetValue(col.ColumnName, i, dr[j].ToString());
                        j++;
                    }
                    i++;
                    prg.Value = i;

                }

                oForm.Freeze(true);
                mtObj.LoadFromDataSource();
                oForm.Freeze(false);
                prg.Stop();
            }
            catch (Exception ex)
            {
                prg.Stop();
            }
        }
        private void pullDepartmentsFromFile()
        {

        }
        private void fillObj()
        {
            cbImpObj.ValidValues.Add("-1", "[Select One]");
            cbImpObj.ValidValues.Add("1", "Department");
            cbImpObj.ValidValues.Add("2", "Locations");
            cbImpObj.ValidValues.Add("3", "Branches");
            cbImpObj.ValidValues.Add("4", "Designations");
            cbImpObj.ValidValues.Add("5", "Positions");
            cbImpObj.ValidValues.Add("6", "Employee");

            cbImpObj.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
        }

        private void ImportDataInHRMS()
        {
            switch (cbImpObj.Value.Trim())
            {
                case "1":
                    postDepartments();
                    break;
                case "2":
                    postLocations();
                    break;
                case "3":
                    postBranchs();
                    break;
                case "4":
                    postDesignations();
                    break;
                case "5":
                    postPositions();
                    break;
                case "6":
                    postEmployee();
                    break;
            }
        }
        private void postLocations()
        {

            for (int i = 0; i < dtMat.Rows.Count; i++)
            {
                string strCode = dtMat.GetValue("Name", i);

                int cnt = (from p in dbHrPayroll.MstLocation where p.Name == strCode.Trim() select p).Count();
                if (cnt > 0)
                {
                }
                else
                {
                    MstLocation Loc = new MstLocation();
                    Loc.Name = strCode.Trim();
                    Loc.Description = dtMat.GetValue("Description", i);
                    Loc.CreateDate = DateTime.Now;
                    Loc.UpdateDate = DateTime.Now;
                    Loc.UserId = oCompany.UserName;
                    Loc.UpdatedBy = oCompany.UserName;
                    dbHrPayroll.MstLocation.InsertOnSubmit(Loc);
                }
            }
            dbHrPayroll.SubmitChanges();

            oApplication.MessageBox("Location imported successfully");
        }
        private void postDepartments()
        {

            for (int i = 0; i < dtMat.Rows.Count; i++)
            {
                string strCode = dtMat.GetValue("Code", i);

                int cnt = (from p in dbHrPayroll.MstDepartment where p.Code == strCode.Trim() select p).Count();

                if (cnt > 0)
                {
                }
                else
                {
                    MstDepartment dept = new MstDepartment();
                    dept.Code = strCode.Trim();
                    dept.DeptName = dtMat.GetValue("DeptName", i);
                    dept.DeptLevel = 1;
                    dept.FlgActive = true;
                    dept.CreateDate = DateTime.Now;
                    dept.UpdateDate = DateTime.Now;
                    dept.UserId = oCompany.UserName;
                    dept.UpdatedBy = oCompany.UserName;
                    dept.ParentDepartment = 1;
                    dbHrPayroll.MstDepartment.InsertOnSubmit(dept);
                }
            }
            dbHrPayroll.SubmitChanges();

            oApplication.MessageBox("Department imported successfully");
        }
        private void postBranchs()
        {

            for (int i = 0; i < dtMat.Rows.Count; i++)
            {
                string strCode = dtMat.GetValue("Name", i);

                int cnt = (from p in dbHrPayroll.MstBranches where p.Name == strCode.Trim() select p).Count();
                if (cnt > 0)
                {
                }
                else
                {
                    MstBranches branch = new MstBranches();
                    branch.Name = strCode.Trim();
                    branch.Description = dtMat.GetValue("Description", i);
                    branch.CreateDate = DateTime.Now;
                    branch.UpdateDate = DateTime.Now;
                    branch.UserID = oCompany.UserName;
                    branch.UpdatedBy = oCompany.UserName;
                    dbHrPayroll.MstBranches.InsertOnSubmit(branch);
                }
            }
            dbHrPayroll.SubmitChanges();

            oApplication.MessageBox("Branches imported successfully");
        }
        private void postDesignations()
        {

            for (int i = 0; i < dtMat.Rows.Count; i++)
            {
                string strCode = dtMat.GetValue("Name", i);

                int cnt = (from p in dbHrPayroll.MstDesignation where p.Name == strCode.Trim() select p).Count();
                if (cnt > 0)
                {
                }
                else
                {
                    MstDesignation designation = new MstDesignation();
                    designation.Name = strCode.Trim();
                    designation.Description = dtMat.GetValue("Description", i);
                    designation.CreateDate = DateTime.Now;
                    designation.UpdateDate = DateTime.Now;
                    designation.UserId = oCompany.UserName;
                    designation.UpdatedBy = oCompany.UserName;
                    dbHrPayroll.MstDesignation.InsertOnSubmit(designation);
                }
            }
            dbHrPayroll.SubmitChanges();

            oApplication.MessageBox("Designations imported successfully");
        }
        private void postPositions()
        {

            for (int i = 0; i < dtMat.Rows.Count; i++)
            {
                string strCode = dtMat.GetValue("Name", i);

                int cnt = (from p in dbHrPayroll.MstPosition where p.Name == strCode.Trim() select p).Count();
                if (cnt > 0)
                {
                }
                else
                {
                    MstPosition position = new MstPosition();
                    position.Name = strCode.Trim();
                    position.Description = dtMat.GetValue("Description", i);
                    position.CreateDate = DateTime.Now;
                    position.UpdateDate = DateTime.Now;
                    position.UserId = oCompany.UserName;
                    position.UpdatedBy = oCompany.UserName;
                    dbHrPayroll.MstPosition.InsertOnSubmit(position);
                }
            }
            dbHrPayroll.SubmitChanges();

            oApplication.MessageBox("Positions imported successfully");
        }
        private void postEmployee()
        {
            Int32 ErrorAtLine = 0;
            String EmpID = "";
            try
            {

                for (int i = 0; i < dtMat.Rows.Count; i++)
                {
                    string strCode = dtMat.GetValue("EmpID", i);
                    strCode = strCode.Trim();
                    MstEmployee employee;
                    MstUsers empuser;
                    string usercode = dtMat.GetValue("UserCode", i);

                    int cnt = (from p in dbHrPayroll.MstEmployee where p.EmpID == strCode.Trim() select p).Count();
                    if (cnt > 0)
                    {
                        employee = (from p in dbHrPayroll.MstEmployee where p.EmpID == strCode.Trim() select p).FirstOrDefault();
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
                        }


                    }
                    else
                    {


                        employee = new MstEmployee();
                        empuser = new MstUsers();
                        
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
                            employee.FlgActive = true;
                        }
                        employee.FlgUser = true;
                        employee.IntSboTransfered = false;
                        employee.IntSboPublished = true;
                        if (usercode.Trim() == "")
                        {
                            empuser.UserCode = strCode.Trim();
                            empuser.UserID = strCode.Trim();
                            empuser.PassCode = strCode;
                        }
                        else
                        {
                            empuser.UserCode = dtMat.GetValue("UserCode", i);
                            empuser.UserID = dtMat.GetValue("UserCode", i);
                            empuser.PassCode = dtMat.GetValue("Password", i);
                        }

                        employee.EmpID = strCode.Trim();
                        EmpID = strCode.Trim();
                        employee.CreateDate = DateTime.Now;
                        employee.CreatedBy = oCompany.UserName;
                        employee.MstUsers.Add(empuser);
                        dbHrPayroll.MstEmployee.InsertOnSubmit(employee);
                    }



                    employee.UpdateDate = DateTime.Now;
                    employee.UpdatedBy = oCompany.UserName;

                    employee.FirstName = dtMat.GetValue("FirstName", i);
                    employee.LastName = dtMat.GetValue("LastName", i);
                    employee.MiddleName = dtMat.GetValue("MiddleName", i);
                    employee.JobTitle = dtMat.GetValue("JobTitle", i);
                    employee.Initials = dtMat.GetValue("Initials", i);
                    employee.NamePrefix = dtMat.GetValue("NamePrefix", i);
                    employee.OfficePhone = dtMat.GetValue("OfficePhone", i);
                    employee.OfficeExtension = dtMat.GetValue("OfficeExtension", i);
                    employee.OfficeMobile = dtMat.GetValue("OfficeMobile", i);
                    employee.HomePhone = dtMat.GetValue("HomePhone", i);
                    employee.Fax = dtMat.GetValue("Fax", i);
                    employee.OfficeEmail = dtMat.GetValue("OfficeEmail", i);
                    employee.AccountTitle = dtMat.GetValue("AccountTitle", i);
                    employee.BankName = dtMat.GetValue("BankName", i);
                    employee.BranchName = dtMat.GetValue("BranchName", i);
                    employee.AccountNo = dtMat.GetValue("AccountNo", i);
                    employee.FatherName = dtMat.GetValue("FatherName", i);
                    employee.MotherName = dtMat.GetValue("MotherName", i);
                    employee.SocialSecurityNo = dtMat.GetValue("SocialSecurityNo", i);
                    employee.Nationality = dtMat.GetValue("Nationality", i);
                    employee.PassportNo = dtMat.GetValue("PassportNo", i);
                    employee.IncomeTaxNo = dtMat.GetValue("IncomeTaxNo", i);
                    employee.IDNo = dtMat.GetValue("IDNo", i);                    
                    employee.IDIssuedBy = dtMat.GetValue("IDIssuedBy", i);
                    employee.IDPlaceofIssue = dtMat.GetValue("IDPlaceofIssue", i);
                    

                    // Arabic Values
                    employee.EnglishName = dtMat.GetValue("EnglishNameH", i);
                    employee.ArabicName = dtMat.GetValue("ArabicNameH", i);
                    employee.PassportExpiryDt = dtMat.GetValue("PassportExpiryDateH", i);
                    
                    employee.IDExpiryDt = dtMat.GetValue("IDExpiryDateH", i);
                    employee.MedicalCardExpDt = dtMat.GetValue("MedicalCardExpiryDateH", i);
                    employee.DrvLicCompletionDt = dtMat.GetValue("DrvLicCompletionDateH", i);
                    employee.DrvLicLastDt = dtMat.GetValue("DrvLicLastDateH", i);
                    employee.DrvLicReleaseDt = dtMat.GetValue("DrvLicReleaseDateH", i);
                    employee.VisaNo = dtMat.GetValue("VisaNo", i);
                    employee.IqamaProfessional = dtMat.GetValue("IqamaProfessional", i);
                    employee.BankCardExpiryDt = dtMat.GetValue("BankCardExpiry", i);
                    employee.ImgPath = dtMat.GetValue("ImagePath", i);
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

                    //Designation
                    if (dtMat.GetValue("DesignationName", i) != "")
                    {
                        strcode = dtMat.GetValue("DesignationName", i);
                        lnkcnt = (from p in dbHrPayroll.MstDesignation where p.Name == strcode select p).Count();
                        if (lnkcnt > 0)
                        {
                            employee.MstDesignation = (from p in dbHrPayroll.MstDesignation where p.Name == strcode select p).Single();
                            employee.DesignationName = strcode;
                        }
                    }
                    //Position
                    if (dtMat.GetValue("PositionName", i) != "")
                    {
                        strcode = dtMat.GetValue("PositionName", i);
                        lnkcnt = (from p in dbHrPayroll.MstPosition where p.Name == strcode select p).Count();
                        if (lnkcnt > 0)
                        {
                            MstPosition pos = (from p in dbHrPayroll.MstPosition where p.Name == strcode select p).Single();
                            employee.PositionName = strcode;
                            employee.PositionID = pos.Id;
                        }
                    }
                    //Department
                    if (dtMat.GetValue("DepartmentName", i) != "")
                    {
                        strcode = dtMat.GetValue("DepartmentName", i);
                        lnkcnt = (from p in dbHrPayroll.MstDepartment where p.Code == strcode select p).Count();
                        if (lnkcnt > 0)
                        {
                            employee.MstDepartment = (from p in dbHrPayroll.MstDepartment where p.Code == strcode select p).Single();
                            employee.DepartmentName = strcode;
                        }
                    }
                    //Location
                    if (dtMat.GetValue("LocationName", i) != "")
                    {
                        strcode = dtMat.GetValue("LocationName", i);
                        lnkcnt = (from p in dbHrPayroll.MstLocation where p.Name == strcode select p).Count();
                        if (lnkcnt > 0)
                        {
                            employee.MstLocation = (from p in dbHrPayroll.MstLocation where p.Name == strcode select p).Single();
                            employee.LocationName = strcode;
                        }
                    }
                    //Branch
                    if (dtMat.GetValue("BranchName", i) != "")
                    {
                        strcode = dtMat.GetValue("BranchName", i);
                        lnkcnt = (from p in dbHrPayroll.MstBranches where p.Name == strcode select p).Count();
                        if (lnkcnt > 0)
                        {
                            MstBranches brn = (from p in dbHrPayroll.MstBranches where p.Name == strcode select p).Single();
                            employee.BranchID = brn.Id;
                            employee.BranchName = strcode;
                        }
                    }

                    //Payroll
                    if (dtMat.GetValue("PayrollName", i) != "")
                    {
                        strcode = dtMat.GetValue("PayrollName", i);
                        lnkcnt = (from p in dbHrPayroll.CfgPayrollDefination where p.PayrollName == strcode select p).Count();
                        if (lnkcnt > 0)
                        {
                            employee.CfgPayrollDefination = (from p in dbHrPayroll.CfgPayrollDefination where p.PayrollName == strcode select p).Single();
                            employee.PayrollName = strcode;
                        }
                    }

                    //Basic Salary
                    employee.BasicSalary = 0.00M;

                    if (dtMat.GetValue("BasicSalary", i) != "")
                    {
                        try
                        {
                            employee.BasicSalary = Convert.ToDecimal(dtMat.GetValue("BasicSalary", i));
                        }
                        catch { }
                    }

                    //SalaryCurrency
                    if (dtMat.GetValue("SalaryCurrency", i) != "")
                    {
                        string currency = dtMat.GetValue("SalaryCurrency", i);
                        try
                        {
                            employee.SalaryCurrency = dtMat.GetValue("SalaryCurrency", i);
                        }
                        catch { }
                    }

                    //PaymentMode
                    if (dtMat.GetValue("PaymentMode", i) != "")
                    {
                        string PaymentMode = dtMat.GetValue("PaymentMode", i);
                        try
                        {
                            employee.PaymentMode = dtMat.GetValue("PaymentMode", i);
                        }
                        catch { }
                    }

                    //AccountType
                    if (dtMat.GetValue("AccountType", i) != "")
                    {
                        try
                        {
                            employee.AccountType = dtMat.GetValue("AccountType", i);
                        }
                        catch { }
                    }
                    //ReligionID
                    if (dtMat.GetValue("ReligionID", i) != "")
                    {
                        try
                        {
                            employee.ReligionID = dtMat.GetValue("ReligionID", i);
                        }
                        catch { }
                    }
                    //MartialStatusID
                    if (dtMat.GetValue("MartialStatusID", i) != "")
                    {
                        try
                        {
                            employee.MartialStatusID = dtMat.GetValue("MartialStatusID", i);
                        }
                        catch { }
                    }
                    //Gender
                    if (dtMat.GetValue("Gender", i) != "")
                    {
                        try
                        {
                            employee.GenderID = dtMat.GetValue("Gender", i);
                        }
                        catch { }
                    }
                    if (dtMat.GetValue("ContractType", i) != "")
                    {
                        try
                        {
                            employee.EmployeeContractType = dtMat.GetValue("ContractType", i);
                        }
                        catch { }
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
                    }
                    ErrorAtLine = i+1;
                }

                dbHrPayroll.SubmitChanges();

                oApplication.MessageBox("Employee imported successfully");
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Error In Line : " + ErrorAtLine.ToString() + " Employee ID : " + EmpID + " System Error : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
    }

}
