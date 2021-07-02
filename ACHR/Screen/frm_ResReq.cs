using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using System.Globalization;


namespace ACHR.Screen
{
    class frm_ResReq:HRMSBaseForm
    {
        #region Global Variable Area
        
        SAPbouiCOM.Button btSave, btCancel;
        SAPbouiCOM.Item itxtEmployeeCode;
        SAPbouiCOM.EditText txtEmpName, txtEmpCode, txtdocNum, txtManager, txtdoj, txtDocdt, txtdesig, txtSalary, txtOriginator, txtResgdt, txtTerminationDt, txtappStatus;
        SAPbouiCOM.EditText txtResigReason;
        SAPbouiCOM.CheckBox chkPeriodHour, chkPeriodMonth, flgOption3, flgOption4, flgOption5, flgOption6, flgOption7;
        
        private Int32 CurrentRecord = 0, TotalRecords = 0;
        IEnumerable<TrnsResignation> oDocuments = null;
        IEnumerable<MstEmployee> oEmployees = null;
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
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_ResReq Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public override void etBeforeClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etBeforeClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "1":
                    if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE)
                    {
                        if (!AddValidation())
                        {
                            BubbleEvent = false;
                        }
                    }
                    if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_UPDATE_MODE)
                    {
                        if (!UpdateValidation())
                        {
                            BubbleEvent = false;
                        }
                    }
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
                    case "1":
                        //DateTime resigndt, terminationdt;
                        //resigndt = DateTime.ParseExact(txtResgdt.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        //terminationdt = DateTime.ParseExact(txtTerminationDt.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                        //MstEmployee emp = (from p in dbHrPayroll.MstEmployee where p.EmpID == txtEmpCode.Value select p).FirstOrDefault();
                        //var allPeriods = (from a in dbHrPayroll.CfgPeriodDates where a.PayrollId == emp.PayrollID orderby a.StartDate ascending select a).ToList();
                        //CfgPeriodDates PayrollPeriod = (from p in dbHrPayroll.CfgPeriodDates where p.StartDate <= resigndt && p.EndDate >= resigndt && p.PayrollId == emp.PayrollID select p).FirstOrDefault();
                        //if (PayrollPeriod == null)
                        //{
                        //    oApplication.StatusBar.SetText("Resing date does not fall ant Define period ", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        //    return;
                        //}
                        //int previosPeriodID = 0;
                        //int currentPeriodID = 0;
                        //string PreviousMonth = "";
                        //for (int i = 0; i < allPeriods.Count; i++)
                        //{
                        //    if (allPeriods[i].ID == PayrollPeriod.ID)
                        //    {
                        //        previosPeriodID = allPeriods[i - 1].ID;
                        //        PreviousMonth = allPeriods[i - 1].PeriodName;
                        //        currentPeriodID = allPeriods[i].ID;
                        //    }
                        //}
                        //if (previosPeriodID != 0)
                        //{
                        //    var processedsalary = (from s in dbHrPayroll.TrnsSalaryProcessRegister
                        //                           where s.EmpID == emp.ID && s.PayrollPeriodID == currentPeriodID
                        //                           select s).FirstOrDefault();
                        //    if (processedsalary != null)
                        //    {
                        //        var salaryJE = (from a in dbHrPayroll.TrnsJE where a.ID == processedsalary.JENum select a).FirstOrDefault();
                        //        if (salaryJE != null)
                        //        {
                        //            if ((salaryJE.SBOJeNum == null ? 0 : Convert.ToInt32(salaryJE.SBOJeNum)) != 0)
                        //            {
                        //                oApplication.StatusBar.SetText("You can not enter resign as current period already has posted salary: EmployeeID '" + emp.EmpID + "'  ", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        //                return;
                        //            }
                        //            else
                        //            {
                        //                oApplication.StatusBar.SetText("You can not enter resign as current period already has processed salary. ", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        //                return;
                        //            }
                        //        }
                        //        else
                        //        {
                        //            oApplication.StatusBar.SetText("You can not enter resign as current period already has processed salary. ", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        //            return;
                        //        }
                        //    }
                        //    else
                        //    {
                        //        var processedsalaryPrevious = (from s in dbHrPayroll.TrnsSalaryProcessRegister
                        //                                       where s.EmpID == emp.ID && s.PayrollPeriodID == previosPeriodID
                        //                                       select s).FirstOrDefault();
                        //        if (processedsalaryPrevious != null)
                        //        {
                        //            var salaryJE = (from a in dbHrPayroll.TrnsJE where a.ID == processedsalaryPrevious.JENum select a).FirstOrDefault();
                        //            if (salaryJE != null)
                        //            {
                        //                if ((salaryJE.SBOJeNum == null ? 0 : Convert.ToInt32(salaryJE.SBOJeNum)) != 0)
                        //                {
                        //                    int confirm = oApplication.MessageBox("Are you sure you want to Add Resignation Request.? ", 3, "Yes", "No", "Cancel");
                        //                    if (confirm == 2 || confirm == 3) return;
                        //                    if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE)
                        //                    {
                        //                        AddRecord();
                        //                    }
                        //                    else
                        //                    {
                        //                        UpdateRecord();
                        //                    }
                        //                }
                        //            }
                        //        }
                        //        else
                        //        {
                        //            oApplication.StatusBar.SetText("JE should be posted Previous Month " + PreviousMonth + ".", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        //            return;
                        //        }
                        //    }
                        //}

                        int confirm = oApplication.MessageBox("Are you sure you want to Add Resignation Request.? ", 3, "Yes", "No", "Cancel");
                        if (confirm == 2 || confirm == 3) return;
                        if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE)
                        {
                            AddRecord();
                        }
                        else
                        {
                            UpdateRecord();
                        }
                        break;
                    case "btId":
                        OpenNewSearchForm();
                        break;
                    case "2":
                        break;
                    default:
                        break;
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_ResReq Function: AfterClick Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        public override void getNextRecord()
        {
            base.getNextRecord();
            GetNextRecord();
        }
        
        public override void getPreviouRecord()
        {
            base.getPreviouRecord();
            GetPreviosRecord();
        }
        
        public override void etFormAfterActivate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etFormAfterActivate(ref pVal, ref BubbleEvent);
            SetEmpValues();
        }

        public override void FindRecordMode()
        {
            base.FindRecordMode();

            OpenNewSearchForm();
            oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
        }
        #endregion

        #region Local Methods

        public void InitiallizeForm()
        {
            try
            {
                btSave = oForm.Items.Item("1").Specific;
                btCancel = oForm.Items.Item("2").Specific;

                //txtIDDtOfIssue = oForm.Items.Item("txisudt").Specific;
                //oForm.DataSources.UserDataSources.Add("txisudt", SAPbouiCOM.BoDataType.dt_DATE, 20);
                //txtIDDtOfIssue.DataBind.SetBound(true, "", "txisudt");
                //txtIDDtOfIssue.Value = DateTime.Now.ToString("yyyyMMdd");

                txtTerminationDt = oForm.Items.Item("txterdt").Specific;
                oForm.DataSources.UserDataSources.Add("txterdt", SAPbouiCOM.BoDataType.dt_DATE);
                txtTerminationDt.DataBind.SetBound(true, "", "txterdt");


                txtappStatus = oForm.Items.Item("txtappst").Specific;
                oForm.DataSources.UserDataSources.Add("txtappst", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txtappStatus.DataBind.SetBound(true, "", "txtappst");

                //Initializing Textboxes
                txtEmpName = oForm.Items.Item("txtEmpN").Specific;
                oForm.DataSources.UserDataSources.Add("txtEmpN", SAPbouiCOM.BoDataType.dt_LONG_TEXT, 150);
                txtEmpName.DataBind.SetBound(true, "", "txtEmpN");
                
                txtEmpCode = oForm.Items.Item("txtEmpC").Specific;
                oForm.DataSources.UserDataSources.Add("txtEmpC", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txtEmpCode.DataBind.SetBound(true, "", "txtEmpC");
                itxtEmployeeCode = oForm.Items.Item("txtEmpC");
                
                txtdocNum = oForm.Items.Item("txtDocN").Specific;
                oForm.DataSources.UserDataSources.Add("txtDocN", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txtdocNum.DataBind.SetBound(true, "", "txtDocN");
                
                txtManager = oForm.Items.Item("txtMang").Specific;
                oForm.DataSources.UserDataSources.Add("txtMang", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txtManager.DataBind.SetBound(true, "", "txtMang");
                
                oForm.DataSources.UserDataSources.Add("txtdoj", SAPbouiCOM.BoDataType.dt_DATE, 30);
                txtdoj = oForm.Items.Item("txtdoj").Specific;
                txtdoj.DataBind.SetBound(true, "", "txtdoj");

                oForm.DataSources.UserDataSources.Add("txtDocD", SAPbouiCOM.BoDataType.dt_DATE, 30);
                txtDocdt = oForm.Items.Item("txtDocD").Specific;
                txtDocdt.DataBind.SetBound(true, "", "txtDocD");

                oForm.DataSources.UserDataSources.Add("txtDeig", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txtdesig = oForm.Items.Item("txtDeig").Specific;
                txtdesig.DataBind.SetBound(true, "", "txtDeig");

                oForm.DataSources.UserDataSources.Add("txtSal", SAPbouiCOM.BoDataType.dt_SUM, 10);
                txtSalary = oForm.Items.Item("txtSal").Specific;
                txtSalary.DataBind.SetBound(true, "", "txtSal");

                oForm.DataSources.UserDataSources.Add("txtOrig", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txtOriginator = oForm.Items.Item("txtOrig").Specific;
                txtOriginator.DataBind.SetBound(true, "", "txtOrig");

                oForm.DataSources.UserDataSources.Add("txtRoD", SAPbouiCOM.BoDataType.dt_DATE, 30);
                txtResgdt = oForm.Items.Item("txtRoD").Specific;
                txtResgdt.DataBind.SetBound(true, "", "txtRoD");

                txtResigReason = oForm.Items.Item("txtResR").Specific;
                oForm.DataSources.UserDataSources.Add("txtResR", SAPbouiCOM.BoDataType.dt_LONG_TEXT, 100);
                txtResigReason.DataBind.SetBound(true, "", "txtResR");

                oForm.DataSources.UserDataSources.Add("flgOption1", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                chkPeriodHour = oForm.Items.Item("flgOption1").Specific;
                chkPeriodHour.DataBind.SetBound(true, "", "flgOption1");


                oForm.DataSources.UserDataSources.Add("flgOption2", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                chkPeriodMonth = oForm.Items.Item("flgOption2").Specific;
                chkPeriodMonth.DataBind.SetBound(true, "", "flgOption2");

                oForm.DataSources.UserDataSources.Add("flgOption3", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                flgOption3 = oForm.Items.Item("flgOption3").Specific;
                flgOption3.DataBind.SetBound(true, "", "flgOption3");

                oForm.DataSources.UserDataSources.Add("flgOption4", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                flgOption4 = oForm.Items.Item("flgOption4").Specific;
                flgOption4.DataBind.SetBound(true, "", "flgOption4");


                oForm.DataSources.UserDataSources.Add("flgOption5", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                flgOption5 = oForm.Items.Item("flgOption5").Specific;
                flgOption5.DataBind.SetBound(true, "", "flgOption5");


                oForm.DataSources.UserDataSources.Add("flgOption6", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                flgOption6 = oForm.Items.Item("flgOption6").Specific;
                flgOption6.DataBind.SetBound(true, "", "flgOption6");

                oForm.DataSources.UserDataSources.Add("flgOption7", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                flgOption7 = oForm.Items.Item("flgOption7").Specific;
                flgOption7.DataBind.SetBound(true, "", "flgOption7");
                GetDataFilterData();
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("InitializeFrom Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }               

        private void LoadSelectedData(String pCode)
        {

            try
            {
                if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE)
                {
                    string strDocStatus = "LV0001", strApprovalStatus = "LV0005";              
                    if (!String.IsNullOrEmpty(pCode))
                    {
                        var getEmp = (from a in dbHrPayroll.MstEmployee
                                      where a.EmpID == pCode
                                      select a).FirstOrDefault();
                        var GetUser = getEmp.MstUsers.FirstOrDefault();
                        if (getEmp != null)
                        {

                            //txtdocNum.Value = Convert.ToString(dbHrPayroll.TrnsResignation.Count() + 1);
                            txtdocNum.Value = getDocNumber();
                            if (GetUser != null)
                            {
                                txtOriginator.Value = GetUser.UserID; 
                            }
                            txtEmpName.Value = getEmp.FirstName + " " + getEmp.MiddleName + " " + getEmp.LastName;
                            txtManager.Value = (from e in dbHrPayroll.MstEmployee where e.ID == getEmp.Manager select (e.FirstName + " " + e.MiddleName + " " + e.LastName)).FirstOrDefault();
                            txtdoj.Value = getEmp.JoiningDate == null ? "" : Convert.ToDateTime(getEmp.JoiningDate).ToString("yyyyMMdd");
                            txtdesig.Value = getEmp.DesignationName;
                            txtSalary.Value = String.Format("{0:0.00}", getEmp.BasicSalary);
                            //txtResgdt.Value = DateTime.Now.ToString("yyyyMMdd");
                            //txtTerminationDt.Value = DateTime.Now.ToString("yyyyMMdd");
                            txtDocdt.Value = DateTime.Now.ToString("yyyyMMdd");
                            //txtTerminationDt.Value = dbHrPayroll.MstLOVE.Where(lv => lv.Code == strDocStatus).Single().Value;

                            txtappStatus.Value = dbHrPayroll.MstLOVE.Where(lv => lv.Code == strApprovalStatus).Single().Value;                                           
                        }

                    }
                }
                else
                {

                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_AdvncReq Function: LoadSelectedData Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private string getDocNumber()
        {
            string docnumber = string.Empty;
            try
            {
                //int totalRecords = (from a in dbHrPayroll.TrnsResignation select a).Count();
                int totalRecords = dbHrPayroll.TrnsResignation.Max(a => a.Id);
                if (totalRecords > 0)
                {
                    docnumber = (totalRecords + 1).ToString();
                }
                else
                {
                    docnumber = "1";
                }
            }
            catch (Exception ex)
            {
                docnumber = "1";
            }
            return docnumber;
        
        }
        
        private void AddRecord()
        {
            try
            {               
                int EmpID; 
                TrnsResignation oNew = new TrnsResignation();
                var oEmp = (from a in dbHrPayroll.MstEmployee
                              where a.EmpID == txtEmpCode.Value.Trim()
                              select a).FirstOrDefault();
                if (oEmp == null)
                {
                    oApplication.StatusBar.SetText("Please select Valid Employee Code", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                if (string.IsNullOrEmpty(txtResgdt.Value))
                {
                    oApplication.StatusBar.SetText("Please select Valid Resign Date", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                // Chechk For Already Enter Resighn Request
                TrnsResignation AlreadyEnteredResign = (from a in dbHrPayroll.TrnsResignation
                                                                                 where a.DocAprStatus!="LV0006" && a.EmpID==oEmp.ID
                                                                                 select a).FirstOrDefault();               
                if (AlreadyEnteredResign != null)
                {
                    if (AlreadyEnteredResign.DocAprStatus == "LV0006")
                    {
                    }
                    else if (AlreadyEnteredResign.DocAprStatus == "LV0007")
                    {
                    }
                    else
                    {
                        oApplication.StatusBar.SetText("Already Entered Resign Request for Selected Employee", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return;
                    }
                }
                var getUser = oEmp.MstUsers.FirstOrDefault();
                EmpID = oEmp.ID;
                oNew.Series = -1;
                //oNew.DocNum = txtdocNum.Value == string.Empty ? 0 : Convert.ToInt32(txtdocNum.Value);
                oNew.EmpID = oEmp.ID;
                
                if (!string.IsNullOrEmpty(txtManager.Value))
                {
                    oNew.ManagerID = oEmp.Manager;                    
                }                
                if (!string.IsNullOrEmpty(txtdoj.Value))
                {
                    oNew.DateOfJoining = DateTime.ParseExact(txtdoj.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                }             
                oNew.DesignationID = oEmp.DesignationID;
                if (!string.IsNullOrEmpty(txtResgdt.Value))
                {
                    oNew.ResignDate = DateTime.ParseExact(txtResgdt.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                }
                if (!string.IsNullOrEmpty(txtTerminationDt.Value))
                {
                    //TODO : termination date
                    oNew.TerminationDate = DateTime.ParseExact(txtTerminationDt.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                }
                if (getUser != null)
                {
                    oNew.OriginatorID = EmpID;                  
                }
                else
                {
                    oNew.OriginatorID = EmpID;                   
                }
                oNew.ResignationReason = txtResigReason.Value;
                oNew.FlgOption1 = chkPeriodHour.Checked;
                oNew.FlgOption2 = chkPeriodMonth.Checked;
                oNew.EmpTermCount = oEmp.TermCount == null ? 1 : Convert.ToInt32(oEmp.TermCount);
                oNew.UserId = oCompany.UserName;
                oNew.CreateDate = DateTime.Now;
                oNew.UpdatedBy = oCompany.UserName;
                oNew.UpdateDate = DateTime.Now;
                dbHrPayroll.TrnsResignation.InsertOnSubmit(oNew);
                dbHrPayroll.SubmitChanges();
                oNew.DocNum = oNew.Id;
                dbHrPayroll.SubmitChanges();
                oApplication.StatusBar.SetText("Document Added Successfully", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);                
                ClearControls();
                oApplication.StatusBar.SetText("Document Added Successfully", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                GetDataFilterData();
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("InsertResgRequest : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void UpdateRecord()
        {
            try
            {
                TrnsResignation oDoc = dbHrPayroll.TrnsResignation.Where(r => r.DocNum.ToString() == txtdocNum.Value).FirstOrDefault();
                if (oDoc != null)
                {
                    oDoc.ResignDate = DateTime.ParseExact(txtResgdt.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    oDoc.TerminationDate = DateTime.ParseExact(txtTerminationDt.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    oDoc.MstEmployee.TerminationDate = DateTime.ParseExact(txtTerminationDt.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    oDoc.MstEmployee.ResignDate = DateTime.ParseExact(txtResgdt.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    oDoc.MstEmployee.TermCount = oDoc.EmpTermCount;
                    oDoc.FlgOption1 = chkPeriodHour.Checked;
                    oDoc.FlgOption2 = chkPeriodMonth.Checked;
                    
                    oDoc.UpdateDate = DateTime.Now;
                    oDoc.UpdatedBy = oCompany.UserName;
                    dbHrPayroll.SubmitChanges();
                    oApplication.StatusBar.SetText("Document Updated Successfully", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                    GetDataFilterData();
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("UpdateRecord : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
        }

        private Boolean AddValidation()
        {
            try
            {
                string EmpCode = txtEmpCode.Value.Trim();
                var oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == EmpCode select a).FirstOrDefault();
                if (oEmp == null)
                {
                    oApplication.StatusBar.SetText("Employee not found", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                DateTime DOJ, DOR, DOT;
                DOJ = DateTime.ParseExact(txtdoj.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                DOR = DateTime.ParseExact(txtResgdt.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                DOT = DateTime.ParseExact(txtTerminationDt.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                if (DOJ >= DOR || DOJ >= DOT)
                {
                    oApplication.StatusBar.SetText("Date of Joining can't equal or greater than Resign, Termination dates.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                if (DOR > DOT)
                {
                    oApplication.StatusBar.SetText("Date of resignation can't be greater than termination dates.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                Boolean flgPeriodHour = false, flgPeriodMonth = false;
                flgPeriodHour = chkPeriodHour.Checked;
                flgPeriodMonth = chkPeriodMonth.Checked;
                if (flgPeriodMonth && flgPeriodHour)
                {
                    oApplication.StatusBar.SetText("You can't select both period types.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private Boolean UpdateValidation()
        {
            try
            {
                string EmpCode = txtEmpCode.Value.Trim();
                var oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == EmpCode select a).FirstOrDefault();
                if (oEmp == null)
                {
                    oApplication.StatusBar.SetText("Employee not found", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                var oFS = (from a in dbHrPayroll.TrnsFSHead where a.MstEmployee.EmpID == oEmp.EmpID select a).FirstOrDefault();
                if (oFS != null)
                {
                    oApplication.StatusBar.SetText("Document can't be updated, Final Settlement already processed.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                DateTime DOJ, DOR, DOT;
                if (string.IsNullOrEmpty(txtResgdt.Value))
                {
                    oApplication.StatusBar.SetText("Enter valid resign date.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                if (string.IsNullOrEmpty(txtTerminationDt.Value))
                {
                    oApplication.StatusBar.SetText("Enter valid termination date.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                DOJ = DateTime.ParseExact(txtdoj.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                DOR = DateTime.ParseExact(txtResgdt.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                DOT = DateTime.ParseExact(txtTerminationDt.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                if (DOJ >= DOR || DOJ >= DOT)
                {
                    oApplication.StatusBar.SetText("Date of Joining can't equal or greater than Resign, Termination dates.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                if (DOR > DOT)
                {
                    oApplication.StatusBar.SetText("Date of resignation can't be greater than termination dates.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return false;
                }
                Boolean flgPeriodHour = false, flgPeriodMonth = false;
                flgPeriodHour = chkPeriodHour.Checked;
                flgPeriodMonth = chkPeriodMonth.Checked;
                if (flgPeriodMonth && flgPeriodHour)
                {
                    oApplication.StatusBar.SetText("You can't select both period types.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void FillDocument(Int32 DocumentID)
        {
            try
            {
                oDocuments = dbHrPayroll.TrnsResignation.ToList();
                TrnsResignation oDoc = oDocuments.ElementAt<TrnsResignation>(DocumentID);
                var oEmp = (from a in dbHrPayroll.MstEmployee
                              where a.ID == oDoc.EmpID
                              select a).FirstOrDefault();
                if (oEmp != null)
                {
                    if (oEmp.CreatedBy != null)
                    {
                        var GetUser = oEmp.MstUsers.FirstOrDefault();
                        txtOriginator.Value = GetUser.UserID;
                    }
                    txtEmpCode.Value = oEmp.EmpID;
                    txtEmpName.Value = oEmp.FirstName + " " + oEmp.MiddleName + " " + oEmp.LastName;
                    txtManager.Value = (from e in dbHrPayroll.MstEmployee where e.ID == oEmp.Manager select (e.FirstName + " " + e.MiddleName + " " + e.LastName)).FirstOrDefault();
                    txtdoj.Value = oEmp.JoiningDate == null ? "" : Convert.ToDateTime(oEmp.JoiningDate).ToString("yyyyMMdd");
                    txtResgdt.Value = oDoc.ResignDate == null ? "" : Convert.ToDateTime(oDoc.ResignDate).ToString("yyyyMMdd");
                    txtDocdt.Value = oDoc.CreateDate == null ? "" : Convert.ToDateTime(oDoc.CreateDate).ToString("yyyyMMdd");
                    txtdesig.Value = oEmp.DesignationName;
                    txtResigReason.Value = oDoc.ResignationReason;
                    txtSalary.Value = String.Format("{0:0.00}", oEmp.BasicSalary);
                    txtdocNum.Value = Convert.ToString(oDoc.DocNum);
                    chkPeriodHour.Checked = oDoc.FlgOption1 == null ? false : Convert.ToBoolean(oDoc.FlgOption1);
                    chkPeriodMonth.Checked = oDoc.FlgOption2 == null ? false : Convert.ToBoolean(oDoc.FlgOption2);
                    //txtdoc.Value = dbHrPayroll.MstLOVE.Where(l => l.Code == oDoc.DocStatus).Single().Value;
                    txtappStatus.Value = dbHrPayroll.MstLOVE.Where(l => l.Code == oDoc.DocAprStatus).Single().Value;
                    txtTerminationDt.Value = oDoc.TerminationDate == null ? "" : Convert.ToDateTime(oDoc.TerminationDate).ToString("yyyyMMdd");

                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void picEmp()
        {
            PrepareSearchKeyHash();
            string strSql = sqlString.getSql("empResign", SearchKeyVal);
            picker pic = new picker(oApplication, ds.getDataTable(strSql));
            System.Data.DataTable st = pic.ShowInput("Select Employee", "Select Employee for Resignation Request");
            pic = null;
            if (st.Rows.Count > 0)
            {
                txtEmpCode.Value = st.Rows[0][0].ToString();
                LoadSelectedData(txtEmpCode.Value);
            }
        }
        
        private void GetNextRecord()
        {
            var ResignRecords = dbHrPayroll.TrnsResignation.ToList();
            if (ResignRecords != null && ResignRecords.Count > 0)
            {
                TotalRecords = ResignRecords.Count;
                if (CurrentRecord + 1 >= TotalRecords)
                {
                    CurrentRecord = 0;
                }
                else
                {
                    CurrentRecord++;
                }
                FillDocument(CurrentRecord);
            }
            else
            {
                oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("NoRecordFound"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void GetPreviosRecord()
        {
            var ResignRecords = dbHrPayroll.TrnsResignation.ToList();
            if (ResignRecords != null && ResignRecords.Count > 0)
            {
                TotalRecords = ResignRecords.Count;
                if (CurrentRecord - 1 < 0)
                {
                    CurrentRecord = TotalRecords - 1;
                }
                else
                {
                    CurrentRecord--;
                }
                FillDocument(CurrentRecord);
            }
            else
            {
                oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("NoRecordFound"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        public override void PrepareSearchKeyHash()
        {
            base.PrepareSearchKeyHash();
            SearchKeyVal.Clear();
            if (!string.IsNullOrEmpty(txtEmpCode.Value))
            {
                SearchKeyVal.Add("EmpID", txtEmpCode.Value.ToString());
            }
        }
        
        private void OpenNewSearchForm()
        {
            try
            {
                Program.EmpID = "";
                Program.sqlString = "empResign";
                string comName = "MstSearch";
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
                if (!string.IsNullOrEmpty(Program.EmpID))
                {
                    txtEmpCode.Value = Program.EmpID;
                    LoadSelectedData(txtEmpCode.Value);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void ClearControls()
        {
            try
            {
                GetDataFilterData();
                txtEmpCode.Value = string.Empty;
                txtEmpName.Value = string.Empty;
                txtdocNum.Value = string.Empty;
                txtManager.Value = string.Empty;
                txtdoj.Value = string.Empty;
                txtDocdt.Value = string.Empty;
                txtdesig.Value = string.Empty;
                txtSalary.Value = string.Empty;
                txtOriginator.Value = string.Empty;
                txtResgdt.Value = string.Empty;               
                txtTerminationDt.Value = string.Empty;
                txtappStatus.Value = string.Empty;
                txtResigReason.Value = string.Empty;
                chkPeriodHour.Checked = false;
                chkPeriodMonth.Checked = false;
                flgOption3.Checked = false;
                flgOption4.Checked = false;
                flgOption5.Checked = false;
                flgOption5.Checked = false;
                flgOption6.Checked = false;
                flgOption7.Checked = false;
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("ClearControls : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
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
