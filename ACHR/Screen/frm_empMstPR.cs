using System;
using System.Linq;
using System.Data;

using DIHRMS;
using System.Collections;
using System.Collections.Generic;
using DIHRMS.Custom;


namespace ACHR.Screen
{
    partial class frm_empMstPR : HRMSBaseForm
    {
        IEnumerable<MstEmployee> oEmployees;
        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.Freeze(true);
            InitiallizeForm();
            fillCbs();

            oForm.Freeze(false);
            getData();
            AddNewRecord();
        }
        private void getData()
        {
            CodeIndex.Clear();
            oEmployees = from a in dbHrPayroll.MstEmployee select a;
            Int32 i = 0;
            foreach (MstEmployee oEmp in oEmployees)
            {
                CodeIndex.Add(oEmp.ID.ToString(), i);
                i++;
            }
            totalRecord = i;

        }
        public override void fillFields()
        {
            base.fillFields();
            try
            {
                MstEmployee oEmp = oEmployees.ElementAt<MstEmployee>(currentRecord);
                dtHead.SetValue("ID", 0, oEmp.ID.ToString());
                dtHead.SetValue("empcode", 0, oEmp.EmpID == null ? "" : oEmp.EmpID.ToString());
                dtHead.SetValue("fname", 0, oEmp.FirstName == null ? "" : oEmp.FirstName.ToString());
                dtHead.SetValue("lname", 0, oEmp.LastName == null ? "" : oEmp.LastName.ToString());
                dtHead.SetValue("job", 0, oEmp.JobTitle == null ? "" : oEmp.JobTitle.ToString());
                if (oEmp.MstUsers.Count > 0)
                {
                    dtHead.SetValue("login", 0, oEmp.MstUsers.ElementAt(0).UserCode.ToString());
                    dtHead.SetValue("pwd", 0, oEmp.MstUsers.ElementAt(0).PassCode.ToString());
                }
                dtHead.SetValue("location", 0, oEmp.Location == null ? "-1" : oEmp.Location.ToString());
                dtHead.SetValue("position", 0, oEmp.PositionID == null ? "-1" : oEmp.PositionID.ToString());
                dtHead.SetValue("department", 0, oEmp.DepartmentID == null ? "-1" : oEmp.DepartmentID.ToString());
                dtHead.SetValue("branch", 0, oEmp.BranchID == null ? "-1" : oEmp.BranchID.ToString());
                dtHead.SetValue("manager", 0, oEmp.Manager == null ? "-1" : oEmp.Manager.ToString());
                dtHead.SetValue("designation", 0, oEmp.DesignationID == null ? "-1" : oEmp.DesignationID.ToString());
                dtHead.SetValue("sbocode", 0, oEmp.SBOEmpCode == null ? "-1" : oEmp.SBOEmpCode.ToString());

                dtHead.SetValue("initial", 0, oEmp.Initials == null ? "" : oEmp.Initials.ToString());
                dtHead.SetValue("namepre", 0, oEmp.NamePrefix == null ? "" : oEmp.NamePrefix.ToString());
                dtHead.SetValue("officephone", 0, oEmp.OfficePhone == null ? "" : oEmp.OfficePhone.ToString());
                dtHead.SetValue("ext", 0, oEmp.OfficeExtension == null ? "" : oEmp.OfficeExtension.ToString());
                dtHead.SetValue("mobphone", 0, oEmp.OfficeMobile == null ? "" : oEmp.OfficeMobile.ToString());
                dtHead.SetValue("pager", 0, oEmp.Pager == null ? "" : oEmp.Pager.ToString());
                dtHead.SetValue("homephone", 0, oEmp.HomePhone == null ? "" : oEmp.HomePhone.ToString());
                dtHead.SetValue("fax", 0, oEmp.Fax == null ? "" : oEmp.Fax.ToString());
                dtHead.SetValue("email", 0, oEmp.OfficeEmail == null ? "" : oEmp.OfficeEmail.ToString());
                dtHead.SetValue("active", 0, oEmp.FlgActive == true ? "Y" : "N");
                dtHead.SetValue("mname", 0, oEmp.MiddleName == null ? "" : oEmp.MiddleName.ToString());

                dtpayroll.SetValue("basic", 0, oEmp.BasicSalary == null ? "0.00" : oEmp.BasicSalary.ToString());
                dtpayroll.SetValue("currency", 0, oEmp.SalaryCurrency == null ? "" : oEmp.SalaryCurrency.ToString());
                dtpayroll.SetValue("calander", 0, oEmp.EmpCalender == null ? "" : oEmp.EmpCalender.ToString());
                //dtpayroll.SetValue("shift", 0, oEmp..ToString());
                dtpayroll.SetValue("doj", 0, oEmp.JoiningDate == null ? "" : Convert.ToDateTime(oEmp.JoiningDate).ToString("yyyyMMdd"));
                dtpayroll.SetValue("pmtmode", 0, oEmp.PaymentMode == null ? "-1" : oEmp.PaymentMode.ToString());
                dtpayroll.SetValue("payroll", 0, oEmp.PayrollID == null ? "-1" : oEmp.PayrollID.ToString());
                dtpayroll.SetValue("accttitle", 0, oEmp.AccountTitle == null ? "" : oEmp.AccountTitle.ToString());
                dtpayroll.SetValue("bankname", 0, oEmp.BankName == null ? "" : oEmp.BankName.ToString());
                dtpayroll.SetValue("branchname", 0, oEmp.BankBranch == null ? "" : oEmp.BankBranch.ToString());
                dtpayroll.SetValue("acctnumber", 0, oEmp.AccountNo == null ? "" : oEmp.AccountNo.ToString());
                dtpayroll.SetValue("accttype", 0, oEmp.AccountType == null ? "-1" : oEmp.AccountType.ToString());
                dtpayroll.SetValue("effectdate", 0, oEmp.EffectiveDate == null ? "" : Convert.ToDateTime(oEmp.EffectiveDate).ToString("yyyyMMdd"));
                dtpayroll.SetValue("percent", 0, oEmp.PercentagePaid == null ? "0.00" : oEmp.PercentagePaid.ToString());

                dtpersonal.SetValue("fathername", 0, oEmp.FatherName == null ? "" : oEmp.FatherName.ToString());
                dtpersonal.SetValue("mothername", 0, oEmp.MotherName == null ? "" : oEmp.MotherName.ToString());
                dtpersonal.SetValue("religion", 0, oEmp.ReligionID == null ? "-1" : oEmp.ReligionID.ToString());
                dtpersonal.SetValue("maritalstatus", 0, oEmp.MartialStatusID == null ? "-1" : oEmp.MartialStatusID.ToString());
                dtpersonal.SetValue("socialsecurity", 0, oEmp.SocialSecurityNo == null ? "" : oEmp.SocialSecurityNo.ToString());
                dtpersonal.SetValue("unionmember", 0, oEmp.EmpUnion == null ? "" : oEmp.EmpUnion.ToString());
                dtpersonal.SetValue("unionnumber", 0, oEmp.UnionMembershipNo == null ? "" : oEmp.UnionMembershipNo.ToString());
                dtpersonal.SetValue("nationality", 0, oEmp.Nationality == null ? "" : oEmp.Nationality.ToString());
                dtpersonal.SetValue("passport", 0, oEmp.PassportNo == null ? "" : oEmp.PassportNo.ToString());
                dtpersonal.SetValue("pasportdoi", 0, oEmp.PassportDateofIssue == null ? "" : Convert.ToDateTime(oEmp.PassportDateofIssue).ToString("yyyyMMdd"));
                dtpersonal.SetValue("passportexpdate", 0, oEmp.PassportExpiryDate == null ? "" : Convert.ToDateTime(oEmp.PassportExpiryDate).ToString("yyyyMMdd"));
                dtpersonal.SetValue("itaxnum", 0, oEmp.IncomeTaxNo == null ? "" : oEmp.IncomeTaxNo.ToString());
                dtpersonal.SetValue("idcardno", 0, oEmp.IDNo == null ? "" : oEmp.IDNo.ToString());
                dtpersonal.SetValue("idcardissuedate", 0, oEmp.IDDateofIssue == null ? "" : Convert.ToDateTime(oEmp.IDDateofIssue).ToString("yyyyMMdd"));
                dtpersonal.SetValue("idcardissuer", 0, oEmp.IDPlaceofIssue == null ? "" : oEmp.IDPlaceofIssue.ToString());
                dtpersonal.SetValue("idissueby", 0, oEmp.IDIssuedBy == null ? "" : oEmp.IDIssuedBy.ToString());
                dtpersonal.SetValue("idexpdate", 0, oEmp.IDExpiryDate == null ? "" : Convert.ToDateTime(oEmp.IDExpiryDate).ToString("yyyyMMdd").ToString());
                txFname.Active = true;
                ItxEmpCode.Enabled = false;
                ItxLoginId.Enabled = false;
            }
            catch(Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
            }
            oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;

        }
        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {


            base.etAfterClick(ref pVal, ref BubbleEvent);
            try
            {

                switch (pVal.ItemUID)
                {


                    case "1":
                        doSubmit();
                        break;
                }
            }

            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
            }
        }
       
        public override void FindRecordMode()
        {
            base.FindRecordMode();
            iniControlls();
            ItxEmpCode.Enabled = true;
            ItxLoginId.Enabled = true;
            txEmpCode.Active = true;
        }
        private void fillCbs()
        {
            try
            {

                FillPositionCombo();
                FillDepartmentCombo();
                FillBranchCombo();
                FillManagerCombo();
                FillLocationsCombo();
                FillDesignationCombo();
                FillLovList(cbMarStat, "Marital");
                FillLovList(cbReligion, "Religion");
                FillLovList(cbCurr, "SalaryCurrency");
                FillLovList(cbPmtMode, "PaymentMode");
                FillLovList(cbAccType, "AccountType");
                FillPayrollCombo();
                FillSboUsrCombo();
            }


            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
            }
        }
        private void FillPositionCombo()
        {
            try
            {
                var AllPositions = from a in dbHrPayroll.MstPosition select new { Id = a.Id, Name = a.Name };
                cbPos.ValidValues.Add("-1", "");
                foreach (var Position in AllPositions)
                {
                    cbPos.ValidValues.Add(Convert.ToString(Position.Id), Convert.ToString(Position.Name));
                }
                cbPos.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }
        private void FillDepartmentCombo()
        {
            try
            {
                var AllDepartment = from a in dbHrPayroll.MstDepartment select new { ID = a.ID, DeptName = a.DeptName };
                cbDept.ValidValues.Add("-1", "");
                foreach (var Dept in AllDepartment)
                {
                    cbDept.ValidValues.Add(Convert.ToString(Dept.ID), Convert.ToString(Dept.DeptName));
                }
                cbDept.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }
        private void FillDesignationCombo()
        {
            try
            {
                cbDesign.ValidValues.Add("-1","");
                var Designations = from a in dbHrPayroll.MstDesignation select new { Id = a.Id, Name = a.Name };
                foreach (var One in Designations)
                {
                    cbDesign.ValidValues.Add(Convert.ToString(One.Id), Convert.ToString(One.Name));
                }
                cbDesign.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }
        private void FillBranchCombo()
        {
            try
            {
                var AllBranches = from a in dbHrPayroll.MstBranches select new { Id = a.Id, Name = a.Name };
                cbBranch.ValidValues.Add("-1", "");
                foreach (var Branch in AllBranches)
                {
                    cbBranch.ValidValues.Add(Convert.ToString(Branch.Id), Convert.ToString(Branch.Name));
                }
                cbBranch.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }
        private void FillManagerCombo()
        {
            cbMgr.ValidValues.Add("-1", "");
            var AllEmployee = from a in dbHrPayroll.MstEmployee select new { ID = a.ID, FirstName = a.FirstName, LastName = a.LastName };
            foreach (var Emp in AllEmployee)
            {
                cbMgr.ValidValues.Add(Convert.ToString(Emp.ID), Convert.ToString(Emp.FirstName + " " + Emp.LastName));
            }
            cbMgr.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

        }
        private void FillLovList(SAPbouiCOM.ComboBox pCombo, String TypeCode)
        {
            try
            {
                pCombo.ValidValues.Add("-1", "");
                var MartialStatus = from a in dbHrPayroll.MstLOVE where a.Type.Contains(TypeCode) select new { Code = a.Code, Value = a.Value };
                foreach (var One in MartialStatus)
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
        private void FillLocationsCombo()
        {
            try
            {
                cbLoc.ValidValues.Add("-1", "");
                var Locations = from a in dbHrPayroll.MstLocation select new { Id = a.Id, Name = a.Name };
                foreach (var Location in Locations)
                {
                    cbLoc.ValidValues.Add(Convert.ToString(Location.Id), Convert.ToString(Location.Name));
                }
                cbLoc.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }
        private void FillPayrollCombo()
        {
            try
            {
                var PayrollAll = from a in dbHrPayroll.CfgPayrollDefination select new { ID = a.ID, PayrollName = a.PayrollName };
                cbPayroll.ValidValues.Add("-1","");
                foreach (var Prl in PayrollAll)
                {
                    cbPayroll.ValidValues.Add(Convert.ToString(Prl.ID), Convert.ToString(Prl.PayrollName));
                }
                //pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
            cbPayroll.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

        }
        private void FillSboUsrCombo()
        {
            try
            {
                string strSql = "select user_code , U_NAME from " + oCompany.CompanyDB + ".dbo.ousr";
                DataTable dtUsr = ds.getDataTable(strSql);

                cbSBOUsr.ValidValues.Add("-1", "");
                foreach (DataRow  dr in dtUsr.Rows)
                {
                    cbSBOUsr.ValidValues.Add(Convert.ToString(dr[0].ToString()), Convert.ToString(dr[1].ToString()));
                }
                //pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
            cbSBOUsr.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

        }
        public override void AddNewRecord()
        {
            base.AddNewRecord();
            oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            iniControlls();
            //dtHead.Rows .Clear();
            //dtpayroll.Rows.Clear();
            //dtpersonal.Rows.Clear();
            //dtHead.Rows.Add(1);
            //dtpersonal.Rows.Add(1);
            //dtpayroll.Rows.Add(1);

            ItxEmpCode.Enabled = true;
            ItxLoginId.Enabled = true;
            txEmpCode.Active = true;
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
                    if(oForm.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE)
                    {
                        AddNewRecord();
                    }
                    getData();
                }
            }

        }
        public override void PrepareSearchKeyHash()
        {

           
            base.PrepareSearchKeyHash();
            SearchKeyVal.Clear();

           

            SearchKeyVal.Add("emp.EmpID", txEmpCode.Value.ToString().Trim());
            SearchKeyVal.Add("emp.FirstName", Convert.ToString( txFname.Value).ToString().Trim());
            SearchKeyVal.Add("emp.MiddleName", Convert.ToString( txMName.Value).ToString().Trim());
            SearchKeyVal.Add("emp.LastName", Convert.ToString( txLname.Value).ToString().Trim());
            SearchKeyVal.Add("emp.JobTitle", Convert.ToString( txJobT.Value).ToString().Trim());
            if (cbDept.Value.Trim() != "-1") SearchKeyVal.Add("emp.DepartmentID", Convert.ToString( cbDept.Value).ToString().Trim());
            if (cbLoc.Value.Trim() != "-1") SearchKeyVal.Add("emp.Location", Convert.ToString(cbLoc.Value.ToString().Trim()));
            SearchKeyVal.Add("emp.FlgActive", chActive.Checked == true ? "1" : "");

        }
        
        private void doFind()
        {

            PrepareSearchKeyHash();
            string strSql = sqlString.getSql("mstEmployee", SearchKeyVal);
            picker pic = new picker(oApplication, ds.getDataTable(strSql));
            System.Data.DataTable st = pic.ShowInput("Select Employee", "Select  Employee");
            pic = null;
            if (st.Rows.Count > 0)
            {
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                currentObjId = st.Rows[0][0].ToString();
                getRecord(currentObjId);

            }

            // btnMain.Caption = "Ok"
        }
        private bool validateForm()
        {
            bool outResult = true;
            if (txEmpCode.Value == "")
            {
                oApplication.SetStatusBarMessage("Employee Code is required for employee");
                outResult = false;
                return outResult;
            }
            if (txFname.Value == "")
            {
                oApplication.SetStatusBarMessage("First Name is required for employee");
                outResult = false;
                return outResult;
            }
            if(txLoginId.Value=="")
            {
                oApplication.SetStatusBarMessage("Login ID is required");
                outResult = false;
                return outResult;
            }
            else
            {
                if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE)
                {
                    int cntLogin = (from p in dbHrPayroll.MstUsers where p.UserCode == txLoginId.Value.Trim() select p).Count();
                    if (cntLogin > 0)
                    {
                        oApplication.SetStatusBarMessage("Login already exist");
                        outResult = false;
                        return outResult;
                    }
                }
            }

            return outResult;
        }
        private void submitForm()
        {
            try
            {
                MstEmployee oEmp;
                MstUsers ousr;
                int cnt = (from p in dbHrPayroll.MstEmployee where p.EmpID == txEmpCode.Value.Trim() select p).Count();

                if (cnt == 0)
                {
                    oEmp = new MstEmployee();

                    ousr = new MstUsers();
                    oEmp.MstUsers.Add(ousr);
                    ousr.CreateDate = DateTime.Now;
                    ousr.CreatedBy = oCompany.UserName;

                }
                else
                {
                    oEmp = (from p in dbHrPayroll.MstEmployee where p.EmpID == txEmpCode.Value.Trim() select p).Single();
                    ousr = oEmp.MstUsers.ElementAt(0);
                }

                MstLanguages oLan = (from a in dbHrPayroll.MstLanguages where a.Name.Contains(Program.sboLanguage) select a).FirstOrDefault<MstLanguages>();

                ousr.UserCode = txLoginId.Value.Trim();
                ousr.UserID = txLoginId.Value.Trim();
                ousr.MstLanguages = oLan;
                ousr.PassCode = txPwd.Value;
                ousr.MstLanguages = oLan;
                ousr.Language = oLan.Id;
                ousr.FlgActiveUser = true;
                ousr.FlgWebUser = true;
                ousr.UpdateDate = DateTime.Now;
                ousr.UpdatedBy = oCompany.UserName;

                oEmp.EmpID = dtHead.GetValue("empcode", 0);
                oEmp.FirstName = dtHead.GetValue("fname", 0);
                oEmp.LastName = dtHead.GetValue("lname", 0);
                oEmp.JobTitle = dtHead.GetValue("job", 0);
                oEmp.Location = dtHead.GetValue("location", 0) == "-1" ? null : Convert.ToInt16(dtHead.GetValue("location", 0));
                oEmp.PositionID = dtHead.GetValue("position", 0) == "-1" ? null : Convert.ToInt16(dtHead.GetValue("position", 0));
                oEmp.DepartmentID = dtHead.GetValue("department", 0) == "-1" ? null : Convert.ToInt16(dtHead.GetValue("department", 0));
                oEmp.BranchID = Convert.ToInt16(dtHead.GetValue("branch", 0));
                oEmp.Manager = dtHead.GetValue("manager", 0) == "-1" ? null : Convert.ToInt16(dtHead.GetValue("manager", 0));
                oEmp.DesignationID = dtHead.GetValue("designation", 0) == "-1" ? null : Convert.ToInt16(dtHead.GetValue("designation", 0));
                oEmp.Initials = dtHead.GetValue("initial", 0);
                oEmp.NamePrefix = dtHead.GetValue("namepre", 0);
                oEmp.OfficePhone = dtHead.GetValue("officephone", 0);
                oEmp.OfficeExtension = dtHead.GetValue("ext", 0);
                oEmp.OfficeMobile = dtHead.GetValue("mobphone", 0);
                oEmp.Pager = dtHead.GetValue("pager", 0);
                oEmp.HomePhone = dtHead.GetValue("homephone", 0);
                oEmp.Fax = dtHead.GetValue("fax", 0);
                oEmp.OfficeEmail = dtHead.GetValue("email", 0);
                oEmp.FlgActive = dtHead.GetValue("active", 0) == "Y" ? true : false;
                oEmp.MiddleName = dtHead.GetValue("mname", 0);
                oEmp.SBOEmpCode = dtHead.GetValue("sbocode", 0) == "-1" ? null : Convert.ToString(dtHead.GetValue("sbocode", 0));

                oEmp.BasicSalary = Convert.ToDecimal((dtpayroll.GetValue("basic", 0)));
                oEmp.SalaryCurrency = dtpayroll.GetValue("currency", 0);
                oEmp.EmpCalender = dtpayroll.GetValue("calander", 0);
                //dtpayroll.SetValue("shift", 0, oEmp..ToString());

                oEmp.JoiningDate = txDoj.Value == "" ? null : dtpayroll.GetValue("doj", 0);
                oEmp.PaymentMode = dtpayroll.GetValue("pmtmode", 0) == "-1" ? null : dtpayroll.GetValue("pmtmode", 0);
                oEmp.AccountTitle = dtpayroll.GetValue("accttitle", 0);
                oEmp.BankName = dtpayroll.GetValue("bankname", 0);
                oEmp.BankBranch = dtpayroll.GetValue("branchname", 0);
                oEmp.AccountNo = dtpayroll.GetValue("acctnumber", 0);
                oEmp.AccountType = dtpayroll.GetValue("accttype", 0) == "-1" ? null : dtpayroll.GetValue("accttype", 0);
                oEmp.PayrollID = dtpayroll.GetValue("payroll", 0) == "-1" ? null : Convert.ToInt16(dtpayroll.GetValue("payroll", 0));


                oEmp.EffectiveDate = txEffDate.Value == "" ? null : dtpayroll.GetValue("effectdate", 0);
                oEmp.PercentagePaid = Convert.ToInt16(dtpayroll.GetValue("percent", 0));

                oEmp.FatherName = dtpersonal.GetValue("fathername", 0);
                oEmp.MotherName = dtpersonal.GetValue("mothername", 0);
                oEmp.ReligionID = dtpersonal.GetValue("religion", 0) == "-1" ? null : dtpersonal.GetValue("religion", 0);
                oEmp.MartialStatusID = dtpersonal.GetValue("maritalstatus", 0) == "-1" ? null : dtpersonal.GetValue("maritalstatus", 0);
                oEmp.SocialSecurityNo = dtpersonal.GetValue("socialsecurity", 0);
                oEmp.EmpUnion = dtpersonal.GetValue("unionmember", 0);
                oEmp.UnionMembershipNo = dtpersonal.GetValue("unionnumber", 0);
                oEmp.Nationality = dtpersonal.GetValue("nationality", 0);
                oEmp.PassportNo = dtpersonal.GetValue("passport", 0);
                oEmp.PassportDateofIssue = txPsprtDt.Value == "" ? null : dtpersonal.GetValue("pasportdoi", 0);
                oEmp.PassportExpiryDate = txPsprtExp.Value == "" ? null : dtpersonal.GetValue("passportexpdate", 0);
                oEmp.IncomeTaxNo = dtpersonal.GetValue("itaxnum", 0);
                oEmp.IDNo = dtpersonal.GetValue("idcardno", 0);
                oEmp.IDDateofIssue = txIDIsDate.Value == "" ? null : dtpersonal.GetValue("idcardissuedate", 0);
                oEmp.IDPlaceofIssue = dtpersonal.GetValue("idcardissuer", 0);
                oEmp.IDIssuedBy = dtpersonal.GetValue("idissueby", 0);
                oEmp.IDExpiryDate = txIdExpDt.Value == "" ? null : dtpersonal.GetValue("idexpdate", 0);


                dbHrPayroll.SubmitChanges();
                ds.updateStandardElements(oEmp, false);

                oApplication.SetStatusBarMessage(Program.objHrmsUI.getStrMsg("Gen_ChangeSuccess"), SAPbouiCOM.BoMessageTime.bmt_Short, false);


            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage( "Error in updating record:" + ex.Message );

            }
        }
        
    }
   
     
}
