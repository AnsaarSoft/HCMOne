using System;
using System.Data;
using System.Linq;
using DIHRMS;
using System.Collections;
using System.Collections.Generic;
using SAPbobsCOM;
using SAPbouiCOM;


namespace ACHR.Screen
{
    class frm_EmpTrns : HRMSBaseForm
    {

        #region Variable

        SAPbouiCOM.EditText txtDocNum, txtDocDate, txtEmpFrom, txtEmpTo, txtStatus;
        SAPbouiCOM.ComboBox cbExistingLocation, cbToLocation, cbStatus, cbLoc, cbDept, cbDes, cbJob, cbDimension1, cbDimension2, cbDimension3, cbDimension4, cbDimension5;
        SAPbouiCOM.Button btEmpFr, btEmpTo, btGetEmp, btnAdd, btnApplyLocation, btnSaveLocation;
        SAPbouiCOM.Matrix mtEmps;
        SAPbouiCOM.DataTable empDetail, dtPeriods;
        SAPbouiCOM.Column LineID, LocationID, ExLocationID, CostCentreID;
        SAPbouiCOM.CheckBox chkCostCentre, chkDimensions;

        SAPbouiCOM.StaticText lblDimension1, lblDimension2, lblDimension3, lblDimension4, lblDimension5;
        SAPbouiCOM.Item ItxDocNum, itxtDocDate, ItxEmpFrom, ItxEmpTo, IbtnAdd, IbtnApplyLocation, IbtnSaveLocation, IbtGetEmp;
        SAPbouiCOM.Item IcbExistingLocation, IcbToLocation, IcbStatus, IcbLoc, IcbDept, IcbDes, IcbJob, IcbElement;
        SAPbouiCOM.Item ImtEmps;
        SAPbouiCOM.Item ilblDimension1, ilblDimension2, ilblDimension3, ilblDimension4, ilblDimension5;
        SAPbouiCOM.Item icbDimension1, icbDimension2, icbDimension3, icbDimension4, icbDimension5;
        Boolean flgDim1, flgDim2, flgDim3, flgDim4, flgDim5;
        public int currentRecord = 0;
        public int totalRecord = 0;
        public IEnumerable<TrnsEmployeeTransfer> empTransfer;
        public Hashtable CodeIndex = new Hashtable();
        public string SelectedEmp = "";
        Boolean flgEmpFrom, flgEmpTo;

        #endregion

        #region SAP B1 Events

        public override void AddNewRecord()
        {
            base.AddNewRecord();
            InitiallizeDocument();
            oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            oForm.Refresh();
        }

        public override void getNextRecord()
        {
            base.getNextRecord();
            if (currentRecord + 1 == totalRecord)
            {
                currentRecord = 0;
                oApplication.SetStatusBarMessage(Program.objHrmsUI.getStrMsg("Nev_Rec_Last"), SAPbouiCOM.BoMessageTime.bmt_Short, false);

            }
            else
            {
                currentRecord = currentRecord + 1;
            }
            _fillFields();
        }

        public override void getPreviouRecord()
        {
            base.getPreviouRecord();
            if (currentRecord <= 0)
            {
                currentRecord = totalRecord - 1;
            }
            else
            {
                currentRecord = currentRecord - 1;
            }
            _fillFields();
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
                case "btAppLoc":
                    ApplyLocation();
                    break;
                case "btnSaveLoc":
                    UpdateLocation();
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
                txtDocNum = oForm.Items.Item("txDocNum").Specific;
                ItxDocNum = oForm.Items.Item("txDocNum");
                txtDocNum.DataBind.SetBound(true, "", "txDocNum");

                oForm.DataSources.UserDataSources.Add("txdate", SAPbouiCOM.BoDataType.dt_DATE);
                txtDocDate = oForm.Items.Item("txdate").Specific;
                itxtDocDate = oForm.Items.Item("txdate");
                txtDocDate.DataBind.SetBound(true, "", "txdate");

                oForm.DataSources.UserDataSources.Add("txEmpFrom", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Hours Per Day
                txtEmpFrom = oForm.Items.Item("txEmpFrom").Specific;
                ItxEmpFrom = oForm.Items.Item("txEmpFrom");
                txtEmpFrom.DataBind.SetBound(true, "", "txEmpFrom");

                oForm.DataSources.UserDataSources.Add("txEmpTo", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Hours Per Day
                txtEmpTo = oForm.Items.Item("txEmpTo").Specific;
                ItxEmpTo = oForm.Items.Item("txEmpTo");
                txtEmpTo.DataBind.SetBound(true, "", "txEmpTo");

                //oForm.DataSources.UserDataSources.Add("cbExLoc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Hours Per Day
                //cbExistingLocation = oForm.Items.Item("cbExLoc").Specific;
                //IcbExistingLocation = oForm.Items.Item("cbExLoc");
                //cbExistingLocation.DataBind.SetBound(true, "", "cbExLoc");

                oForm.DataSources.UserDataSources.Add("cbToLoc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Hours Per Day
                cbToLocation = oForm.Items.Item("cbToLoc").Specific;
                IcbToLocation = oForm.Items.Item("cbToLoc");
                cbToLocation.DataBind.SetBound(true, "", "cbToLoc");

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


                chkCostCentre = oForm.Items.Item("chkCC").Specific;
                oForm.DataSources.UserDataSources.Add("chkCC", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                chkCostCentre.DataBind.SetBound(true, "", "chkCC");
                chkCostCentre.Checked = true;


                #region Dimensions
                chkDimensions = oForm.Items.Item("chkDIM").Specific;
                oForm.DataSources.UserDataSources.Add("chkDIM", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                chkDimensions.DataBind.SetBound(true, "", "chkDIM");
                chkDimensions.Checked = true;

                cbDimension1 = oForm.Items.Item("cbdim1").Specific;
                icbDimension1 = oForm.Items.Item("cbdim1");
                oForm.DataSources.UserDataSources.Add("cbdim1", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 100);
                cbDimension1.DataBind.SetBound(true, "", "cbdim1");


                cbDimension2 = oForm.Items.Item("cbdim2").Specific;
                icbDimension2 = oForm.Items.Item("cbdim2");
                oForm.DataSources.UserDataSources.Add("cbdim2", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbDimension2.DataBind.SetBound(true, "", "cbdim2");

                cbDimension3 = oForm.Items.Item("cbdim3").Specific;
                icbDimension3 = oForm.Items.Item("cbdim3");
                oForm.DataSources.UserDataSources.Add("cbdim3", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbDimension3.DataBind.SetBound(true, "", "cbdim3");

                cbDimension4 = oForm.Items.Item("cbdim4").Specific;
                icbDimension4 = oForm.Items.Item("cbdim4");
                oForm.DataSources.UserDataSources.Add("cbdim4", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbDimension4.DataBind.SetBound(true, "", "cbdim4");

                cbDimension5 = oForm.Items.Item("cbdim5").Specific;
                icbDimension5 = oForm.Items.Item("cbdim5");
                oForm.DataSources.UserDataSources.Add("cbdim5", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbDimension5.DataBind.SetBound(true, "", "cbdim5");

                lblDimension1 = oForm.Items.Item("1000002").Specific;
                ilblDimension1 = oForm.Items.Item("1000002");
                lblDimension2 = oForm.Items.Item("33").Specific;
                ilblDimension2 = oForm.Items.Item("33");
                lblDimension3 = oForm.Items.Item("35").Specific;
                ilblDimension3 = oForm.Items.Item("35");
                lblDimension4 = oForm.Items.Item("37").Specific;
                ilblDimension4 = oForm.Items.Item("37");
                lblDimension5 = oForm.Items.Item("39").Specific;
                ilblDimension5 = oForm.Items.Item("39");
                #endregion
                empDetail = oForm.DataSources.DataTables.Item("empDetail");
                btEmpFr = oForm.Items.Item("btEmpFr").Specific;
                btEmpTo = oForm.Items.Item("btEmpTo").Specific;
                btGetEmp = oForm.Items.Item("btGetEmp").Specific;
                IbtGetEmp = oForm.Items.Item("btGetEmp");
                btnApplyLocation = oForm.Items.Item("btAppLoc").Specific;
                IbtnApplyLocation = oForm.Items.Item("btAppLoc");
                btnSaveLocation = oForm.Items.Item("btnSaveLoc").Specific;
                IbtnSaveLocation = oForm.Items.Item("btnSaveLoc");
                btnAdd = oForm.Items.Item("1").Specific;
                IbtnAdd = oForm.Items.Item("1");
                //btnSaveLoc
                mtEmps = oForm.Items.Item("mtEmps").Specific;

                LineID = mtEmps.Columns.Item("toloc");
                // LineID.Visible = false;
                LocationID = mtEmps.Columns.Item("toloc");
                ExLocationID = mtEmps.Columns.Item("exloc");
                CostCentreID = mtEmps.Columns.Item("cc");
                //toloc
                AddValidValuesInCombos();
                fillCombo("incStatus", cbStatus);
                fillCbs();
                FillComboDimension1(cbDimension1);
                FillComboDimension2(cbDimension2);
                FillComboDimension3(cbDimension3);
                FillComboDimension4(cbDimension4);
                FillComboDimension5(cbDimension5);
                oForm.PaneLevel = 1;
                AddNewRecord();

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("InitiallizeForm : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            oForm.Freeze(false);
        }

        private void InitiallizeDocument()
        {
            oForm.Freeze(true);
            try
            {
                getData();
                long nextId = ds.getNextId("TrnsEmployeeTransfer", "DoNum");
                txtDocNum.Value = nextId.ToString();
                empDetail.Rows.Clear();
                txtEmpFrom.Value = "";
                txtEmpTo.Value = "";
                txtDocDate.Value = "";
                empDetail.Rows.Clear();
                cbStatus.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

                //txEmpCode.Active = true;
                oForm.Items.Item("1").Enabled = true;

                //IcbStatus.Enabled = true;
                IbtnSaveLocation.Enabled = false;
                IbtnAdd.Enabled = true;
                IbtGetEmp.Enabled = true;
                oForm.Items.Item("btAppLoc").Enabled = true;

                oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                #region Dimensions
                
                cbDimension1.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbDimension1.Item.UniqueID).ValueEx = "-1";
                cbDimension2.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbDimension2.Item.UniqueID).ValueEx = "-1";
                cbDimension3.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbDimension3.Item.UniqueID).ValueEx = "-1";
                cbDimension4.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbDimension4.Item.UniqueID).ValueEx = "-1";
                cbDimension5.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                oForm.DataSources.UserDataSources.Item(cbDimension5.Item.UniqueID).ValueEx = "-1";

                #endregion
            }
            catch (Exception ex)
            {

                oApplication.StatusBar.SetText("InitiallizeDocument : " + ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }
            oForm.Freeze(false);
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

        private void getData()
        {
            CodeIndex.Clear();
            empTransfer = from p in dbHrPayroll.TrnsEmployeeTransfer select p;
            int i = 0;
            foreach (TrnsEmployeeTransfer empTrns in empTransfer)
            {
                CodeIndex.Add(empTrns.ID.ToString(), i);
                i++;
            }
            totalRecord = i;
        }
               
        private void UpdateEmployeeLocation(TrnsEmployeeTransfer loc)
        {

            if (cbStatus.Value.Trim() == "0")
            {
                mtEmps.FlushToDataSource();
                int empCnt = empDetail.Rows.Count;
                for (int i = 0; i < empCnt; i++)
                {
                    string empCode = Convert.ToString(empDetail.GetValue("id", i));
                    MstEmployee emp;
                    emp = (from p in dbHrPayroll.MstEmployee
                           where p.EmpID == empCode
                           select p).FirstOrDefault();

                    #region Get Employee Payroll And Period
                    var oPeriod = (from p in dbHrPayroll.CfgPeriodDates
                                   where emp.PayrollID == p.PayrollId
                                   && p.FlgLocked == false
                                   select p).FirstOrDefault();
                    var ProcessedSalary = (from s in dbHrPayroll.TrnsSalaryProcessRegister
                                           where s.EmpID == emp.ID
                                           && s.PayrollID == emp.PayrollID
                                           && s.PayrollPeriodID == oPeriod.ID
                                           select s).FirstOrDefault();
                    if (ProcessedSalary != null)
                    {
                        var PostedSalary = (from je in dbHrPayroll.TrnsJE
                                            where je.ID == ProcessedSalary.JENum
                                            select je).FirstOrDefault();
                        if (PostedSalary != null)
                        {
                            if (PostedSalary.FlgPosted.GetValueOrDefault() != true)
                            {
                                oApplication.StatusBar.SetText("Salary processed Employee '" + emp.EmpID + "' can't be transfered", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                continue;
                            }
                        }
                        else if (ProcessedSalary != null && PostedSalary == null)
                        {
                            oApplication.StatusBar.SetText("Salary processed Employee '" + emp.EmpID + "' can't be transfered", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            continue;
                        }
                    }
                    #endregion

                    string Exloc = empDetail.GetValue("exloc", i);
                    string ToLoc = empDetail.GetValue("toloc", i);
                    string CostCentre = empDetail.GetValue("cc", i);
                    string Dimension1 = empDetail.GetValue("Dim1",i);
                    string Dimension2 = empDetail.GetValue("Dim2", i);
                    string Dimension3 = empDetail.GetValue("Dim3", i);
                    string Dimension4 = empDetail.GetValue("Dim4", i);
                    string Dimension5 = empDetail.GetValue("Dim5", i);
                    TrnsEmployeeTransfer oDoc = (from p in dbHrPayroll.TrnsEmployeeTransfer where p.DoNum == Convert.ToInt32(txtDocNum.Value) select p).FirstOrDefault();
                    if (oDoc != null)
                    {

                        TrnsEmployeeTransferDetails empTrnsfDetail;
                        int empcount = (from p in dbHrPayroll.TrnsEmployeeTransferDetails where p.ParentID.ToString() == loc.ID.ToString().Trim() select p).Count();
                        if (empcount > 0)
                        {
                            MstLocation empLoc = (from location in dbHrPayroll.MstLocation where location.Description == ToLoc select location).FirstOrDefault();
                            empTrnsfDetail = (from p in dbHrPayroll.TrnsEmployeeTransferDetails where p.ParentID.ToString() == loc.ID.ToString().Trim() && p.EmpID == emp.ID select p).FirstOrDefault();

                            empTrnsfDetail.ToLocation = empDetail.GetValue("toloc", i);
                            empTrnsfDetail.Dimension1 = empDetail.GetValue("Dim1", i);
                            empTrnsfDetail.Dimension2 = empDetail.GetValue("Dim2", i);
                            empTrnsfDetail.Dimension3 = empDetail.GetValue("Dim3", i);
                            empTrnsfDetail.Dimension4 = empDetail.GetValue("Dim4", i);
                            empTrnsfDetail.Dimension5 = empDetail.GetValue("Dim5", i);
                            //empTrnsfDetail.ToLocation = Convert.ToString(empLoc.Id);                          
                            empTrnsfDetail.CostCentre = CostCentre;
                            empTrnsfDetail.UpdateDate = DateTime.Now;
                            empTrnsfDetail.UpdatedBy = oCompany.UserName;
                            loc.UpdateDate = DateTime.Now;
                            loc.UpdatedBy = oCompany.UserName;
                        }
                    }
                }

                //loc.StatusRec = 2;
                dbHrPayroll.SubmitChanges();
                oApplication.StatusBar.SetText("Record Updated Successfully", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                InitiallizeDocument();
                //_fillFields();
            }
        }

        private bool submitForm()
        {
            bool submitResult = true;
            bool assigning = false;
            if (cbStatus.Value.Trim() == "0")
            {
                //int confirm = oApplication.MessageBox("Assigning Location will update the Existing Location of listed employees. Are you sure to assign?", 1, "Yes", "No");
                //if (confirm != 1) return false;
                assigning = true;
            }
            try
            {

                TrnsEmployeeTransfer oDoc;


                int cnt = (from p in dbHrPayroll.TrnsEmployeeTransfer where p.DoNum.ToString() == txtDocNum.Value select p).Count();
                if (cnt > 0)
                {
                    oDoc = (from p in dbHrPayroll.TrnsEmployeeTransfer where p.DoNum == Convert.ToInt32(txtDocNum.Value) select p).FirstOrDefault();
                    if (assigning)
                    {
                        UpdateEmployeeLocation(oDoc);
                        return false;
                    }

                }
                else
                {

                    oDoc = new TrnsEmployeeTransfer();
                    dbHrPayroll.TrnsEmployeeTransfer.InsertOnSubmit(oDoc);
                    oDoc.DoNum = Convert.ToInt32(txtDocNum.Value);
                    if (txtDocDate.Value != "")
                    {
                        oDoc.DocDate = DateTime.ParseExact(txtDocDate.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    }
                    else
                    {
                        oDoc.DocDate = null;
                    }
                    oDoc.StatusRec = Convert.ToInt32(cbStatus.Value);
                    oDoc.CreateDate = DateTime.Now;
                    oDoc.CreatedBy = oCompany.UserName;

                }

                oDoc.UpdateDate = DateTime.Now;
                oDoc.UpdatedBy = oCompany.UserName;

                MstEmployee IncEmp;
                int empCnt = empDetail.Rows.Count;
                if (empCnt == 0) return false;
                for (int i = 0; i < empCnt; i++)
                {
                    string empCode = empDetail.GetValue("id", i);
                    string toLocation = empDetail.GetValue("toloc", i);
                    string exLocation = empDetail.GetValue("exloc", i);
                    if (toLocation == null)
                    {
                        oApplication.StatusBar.SetText("System cannot save record with empty location", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return false;
                    }

                    IncEmp = (from p in dbHrPayroll.MstEmployee where p.EmpID == empCode select p).FirstOrDefault();
                    TrnsEmployeeTransferDetails empTrnsfDetail;
                    int incEmp = (from p in dbHrPayroll.TrnsEmployeeTransferDetails where p.ParentID.ToString() == oDoc.ID.ToString().Trim() select p).Count();
                    if (incEmp > 0)
                    {
                        empTrnsfDetail = (from p in dbHrPayroll.TrnsEmployeeTransferDetails where p.ParentID.ToString() == oDoc.ID.ToString().Trim() select p).FirstOrDefault();
                    }
                    else
                    {
                        empTrnsfDetail = new TrnsEmployeeTransferDetails();

                        oDoc.TrnsEmployeeTransferDetails.Add(empTrnsfDetail);

                    }
                    MstLocation empLoc = (from location in dbHrPayroll.MstLocation where location.Name == exLocation select location).FirstOrDefault();
                    empTrnsfDetail.ParentID = oDoc.ID;
                    empTrnsfDetail.EmpID = IncEmp.ID;//empDetail.GetValue("Name", i);
                    empTrnsfDetail.EmpName = empDetail.GetValue("Name", i);

                    empTrnsfDetail.ExistingLocation = Convert.ToString(empLoc.Id);//empDetail.GetValue("exloc", i);
                    empTrnsfDetail.ToLocation = empDetail.GetValue("toloc", i);
                    empTrnsfDetail.CostCentre = empDetail.GetValue("cc", i);

                    empTrnsfDetail.Dimension1 = empDetail.GetValue("Dim1",i);
                    empTrnsfDetail.Dimension2 = empDetail.GetValue("Dim2", i);
                    empTrnsfDetail.Dimension3 = empDetail.GetValue("Dim3", i);
                    empTrnsfDetail.Dimension4 = empDetail.GetValue("Dim4", i);
                    empTrnsfDetail.Dimension5 = empDetail.GetValue("Dim5", i);

                    empTrnsfDetail.CreateDate = DateTime.Now;
                    empTrnsfDetail.CreatedBy = oCompany.UserName;
                    empTrnsfDetail.UpdateDate = DateTime.Now;
                    empTrnsfDetail.UpdatedBy = oCompany.UserName;
                }



                dbHrPayroll.SubmitChanges();
                oApplication.StatusBar.SetText("Record Saved Successfully", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE)
                {
                    InitiallizeDocument();

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

        private bool UpdateLocation()
        {
            bool submitResult = true;
            bool assigning = false;
            if (cbStatus.Value.Trim() == "0")
            {
                int confirm = oApplication.MessageBox("Assigning Location will update the Existing Location of listed employees. Are you sure to assign?", 1, "Yes", "No");
                if (confirm != 1) return false;
                assigning = true;
            }
            try
            {
                TrnsEmployeeTransfer oDoc;

                oDoc = (from p in dbHrPayroll.TrnsEmployeeTransfer
                        where p.DoNum == Convert.ToInt32(txtDocNum.Value)
                        select p).FirstOrDefault();

                if (assigning)
                {
                    MstEmployee TransferEmp;
                    int empCnt = empDetail.Rows.Count;
                    if (empCnt == 0) return false;
                    for (int i = 0; i < empCnt; i++)
                    {
                        string empCode = empDetail.GetValue("id", i);
                        string empCostCenter = empDetail.GetValue("cc", i);
                        string Dimension1 = empDetail.GetValue("Dim1", i);
                        string Dimension2 = empDetail.GetValue("Dim2", i);
                        string Dimension3 = empDetail.GetValue("Dim3", i);
                        string Dimension4 = empDetail.GetValue("Dim4", i);
                        string Dimension5 = empDetail.GetValue("Dim5", i);

                        TransferEmp = (from p in dbHrPayroll.MstEmployee
                                  where p.EmpID == empCode
                                  select p).FirstOrDefault();
                        int TransEmpDetails = (from p in dbHrPayroll.TrnsEmployeeTransferDetails
                                      where p.ParentID.ToString() == oDoc.ID.ToString().Trim()
                                      select p).Count();

                        if (TransEmpDetails > 0)
                        {
                            MstEmployee emp = (from p in dbHrPayroll.MstEmployee
                                               where p.EmpID == empCode
                                               select p).FirstOrDefault();

                            TrnsEmployeeTransferDetails EmpTrnsDetail = (from p in dbHrPayroll.TrnsEmployeeTransferDetails
                                                                         where p.EmpID == emp.ID
                                                                         && p.ParentID.ToString() == oDoc.ID.ToString().Trim()
                                                                         select p).FirstOrDefault();

                            MstLocation empLoc = (from loc in dbHrPayroll.MstLocation
                                                  where loc.Id == Convert.ToInt32(EmpTrnsDetail.ToLocation)
                                                  select loc).FirstOrDefault();

                            TransferEmp.Location = empLoc.Id;
                            TransferEmp.LocationName = empLoc.Name;
                            if (empCostCenter != "")
                            {
                                TransferEmp.CostCenter = empDetail.GetValue("cc", i);
                            }
                            #region Dimensions
                            
                            if (!string.IsNullOrEmpty(Dimension1) && Dimension1 != "-1")
                            {
                                TransferEmp.Dimension1 = Dimension1;
                            }
                            else
                            {
                                TransferEmp.Dimension1 = "";
                            }
                            if (!string.IsNullOrEmpty(Dimension2) && Dimension2 != "-1")
                            {
                                TransferEmp.Dimension2 = Dimension2;
                            }
                            else
                            {
                                TransferEmp.Dimension2 = "";
                            }
                            if (!string.IsNullOrEmpty(Dimension3) && Dimension3 != "-1")
                            {
                                TransferEmp.Dimension3 = Dimension3;
                            }
                            else
                            {
                                TransferEmp.Dimension3 = "";
                            }
                            if (!string.IsNullOrEmpty(Dimension4) && Dimension4 != "-1")
                            {
                                TransferEmp.Dimension4 = Dimension4;
                            }
                            else
                            {
                                TransferEmp.Dimension4 = "";
                            }
                            if (!string.IsNullOrEmpty(Dimension5) && Dimension5 != "-1")
                            {
                                TransferEmp.Dimension5 = Dimension5;
                            }
                            else
                            {
                                TransferEmp.Dimension5 = "";
                            }
                            #endregion
                            TransferEmp.UpdateDate = DateTime.Now;
                            TransferEmp.UpdatedBy = oCompany.UserName;
                        }
                    }
                    oDoc.StatusRec = Convert.ToInt32(cbStatus.Value);
                    oDoc.UpdateDate = DateTime.Now;
                    oDoc.UpdatedBy = oCompany.UserName;
                    oDoc.StatusRec = 2;
                    dbHrPayroll.SubmitChanges();
                    JEEntries();
                    oApplication.StatusBar.SetText("Record Updated Successfully", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                    InitiallizeDocument();
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger(ex);
                submitResult = false;
            }
            return submitResult;

        }

        private void JEEntries()
        {
            try
            {
                if (Convert.ToBoolean(Program.systemInfo.FlgJELocationWise))
                {
                    var oDoc = (from p in dbHrPayroll.TrnsEmployeeTransfer where p.DoNum == Convert.ToInt32(txtDocNum.Value) select p).FirstOrDefault();
                    if (oDoc != null)
                    {
                        if (oDoc.StatusRec == 2)
                        {
                            foreach (var OneLine in oDoc.TrnsEmployeeTransferDetails)
                            {
                                DateTime DocDate = Convert.ToDateTime(oDoc.DocDate);
                                int Line1Value = 0, Line2Value = 0, Line3Value = 0, DocDateDay = 0;
                                DocDateDay = Convert.ToDateTime(oDoc.DocDate).Day;
                                var oEmp = (from a in dbHrPayroll.MstEmployee
                                            where a.ID == Convert.ToInt32(OneLine.EmpID)
                                            select a).FirstOrDefault();
                                var oPeriod = (from a in dbHrPayroll.CfgPeriodDates
                                               where a.PayrollId == oEmp.PayrollID
                                               && a.StartDate <= DocDate && a.EndDate >= DocDate
                                               select a).FirstOrDefault();
                                var CheckPreviousRecord = (from a in dbHrPayroll.TrnsEmpTransferSummary
                                                           where a.EmpID == oEmp.ID
                                                           && a.PeriodID == oPeriod.ID
                                                           select a).ToList();
                                int PeriodDays = Convert.ToInt32((Convert.ToDateTime(oPeriod.EndDate) - Convert.ToDateTime(oPeriod.StartDate)).TotalDays + 1);
                                if (CheckPreviousRecord.Count == 0)
                                {
                                    TrnsEmpTransferSummary oRec1 = new TrnsEmpTransferSummary();
                                    dbHrPayroll.TrnsEmpTransferSummary.InsertOnSubmit(oRec1);
                                    oRec1.EmpID = oEmp.ID;
                                    oRec1.Location = Convert.ToInt32(OneLine.ExistingLocation);
                                    oRec1.DocDate = Convert.ToDateTime(oDoc.DocDate).Day;
                                    oRec1.PeriodDays = PeriodDays;
                                    oRec1.PeriodID = oPeriod.ID;
                                    Line1Value = DocDateDay;
                                    oRec1.DayCount = Line1Value;
                                    oRec1.FactorDistribution = Math.Round(Convert.ToDecimal(Line1Value) / Convert.ToDecimal(PeriodDays), 2);

                                    TrnsEmpTransferSummary oRec2 = new TrnsEmpTransferSummary();
                                    dbHrPayroll.TrnsEmpTransferSummary.InsertOnSubmit(oRec2);
                                    oRec2.EmpID = oEmp.ID;
                                    oRec2.Location = Convert.ToInt32(OneLine.ToLocation);
                                    oRec2.DocDate = Convert.ToDateTime(oDoc.DocDate).Day;
                                    oRec2.PeriodDays = PeriodDays;
                                    oRec2.PeriodID = oPeriod.ID;
                                    Line2Value = PeriodDays - DocDateDay;
                                    oRec2.DayCount = Line2Value;
                                    oRec2.FactorDistribution = Math.Round(Convert.ToDecimal(Line2Value) / Convert.ToDecimal(PeriodDays), 2);
                                }
                                else if (CheckPreviousRecord.Count == 2)
                                {
                                    TrnsEmpTransferSummary oRec1 = CheckPreviousRecord[0];
                                    int TempValue = Convert.ToInt32(oRec1.DayCount);

                                    TrnsEmpTransferSummary oRec2 = CheckPreviousRecord[1];
                                    Line2Value = DocDateDay - TempValue;
                                    oRec2.DayCount = Line2Value;
                                    oRec2.FactorDistribution = Math.Round(Convert.ToDecimal(Line2Value) / Convert.ToDecimal(PeriodDays), 2);

                                    TrnsEmpTransferSummary oRec3 = new TrnsEmpTransferSummary();
                                    dbHrPayroll.TrnsEmpTransferSummary.InsertOnSubmit(oRec3);
                                    oRec3.EmpID = oEmp.ID;
                                    oRec3.Location = Convert.ToInt32(OneLine.ToLocation);
                                    oRec3.DocDate = Convert.ToDateTime(oDoc.DocDate).Day;
                                    oRec3.PeriodDays = PeriodDays;
                                    oRec3.PeriodID = oPeriod.ID;
                                    Line3Value = PeriodDays - DocDateDay;
                                    oRec3.DayCount = Line3Value;
                                    oRec3.FactorDistribution = Math.Round(Convert.ToDecimal(Line3Value) / Convert.ToDecimal(PeriodDays), 2);
                                }
                                dbHrPayroll.SubmitChanges();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private void FillToLocation(SAPbouiCOM.Column OneColumn)
        {
            try
            {
                IEnumerable<MstLocation> objlocation = dbHrPayroll.MstLocation.ToList();
                // IEnumerable<MstDesignation> Designations = from a in dbHrPayroll.MstDesignation select a;
                OneColumn.ValidValues.Add("-1", "");
                foreach (MstLocation Loc in objlocation)
                {
                    OneColumn.ValidValues.Add(Convert.ToString(Loc.Id), Convert.ToString(Loc.Description));
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void fillExistlocation(SAPbouiCOM.Column OneColumn, string empid)
        {
            try
            {
                IEnumerable<MstEmployee> EmpLoc = from p in dbHrPayroll.MstEmployee where p.EmpID == empid select p;
                // IEnumerable<MstDesignation> Designations = from a in dbHrPayroll.MstDesignation select a;

                foreach (MstEmployee emp in EmpLoc)
                {
                    OneColumn.ValidValues.Add(Convert.ToString(emp.LocationName), Convert.ToString(emp.Location));
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void fillTolocation()
        {
            //Existing Location
            //cbToLocation.ValidValues.Add("0", "All");
            IEnumerable<MstLocation> toLoc = from p in dbHrPayroll.MstLocation orderby p.Description ascending select p;

            foreach (MstLocation loc in toLoc)
            {
                cbToLocation.ValidValues.Add(loc.Id.ToString(), loc.Description);

            }
            cbToLocation.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            //====
        }

        private void fillCbs()
        {
            int i = 0;
            string selId = "0";


            IEnumerable<MstDepartment> depts = from p in dbHrPayroll.MstDepartment select p;
            cbDept.ValidValues.Add("0", "All");
            foreach (MstDepartment dept in depts)
            {
                cbDept.ValidValues.Add(dept.ID.ToString(), dept.DeptName);

            }
            cbDept.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);



            cbLoc.ValidValues.Add("0", "All");
            IEnumerable<MstLocation> locs = from p in dbHrPayroll.MstLocation select p;

            foreach (MstLocation loc in locs)
            {
                cbLoc.ValidValues.Add(loc.Id.ToString(), loc.Description);

            }
            cbLoc.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

            cbDes.ValidValues.Add("0", "All");
            IEnumerable<MstDesignation> designations = from p in dbHrPayroll.MstDesignation select p;

            foreach (MstDesignation des in designations)
            {
                cbDes.ValidValues.Add(des.Id.ToString(), des.Description);

            }
            cbDes.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);


            cbJob.ValidValues.Add("0", "All");
            IEnumerable<MstJobTitle> jobtitles = from p in dbHrPayroll.MstJobTitle select p;

            foreach (MstJobTitle jt in jobtitles)
            {
                cbJob.ValidValues.Add(jt.Id.ToString(), jt.Description);

            }
            cbJob.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

            fillTolocation();

        }

        private void getEmployees()
        {
            try
            {

                var Data = dbHrPayroll.MstEmployee.Where(e => e.FlgActive == true && e.PayrollID > 0 && e.ResignDate == null).ToList();

                if (txtEmpFrom.Value != string.Empty && txtEmpTo.Value != string.Empty)
                {
                    int? sortorderfrom = (from a in dbHrPayroll.MstEmployee where a.EmpID == txtEmpFrom.Value.Trim() select a.SortOrder).FirstOrDefault();
                    int? sortorderto = (from a in dbHrPayroll.MstEmployee where a.EmpID == txtEmpTo.Value.Trim() select a.SortOrder).FirstOrDefault();
                    if (sortorderfrom == null) sortorderfrom = 0;
                    if (sortorderto == null) sortorderto = 100000;
                    if (sortorderfrom > sortorderto)
                    {
                        //Data = Data.Where(e => e.SortOrder >= intEmpIdTo && e.SortOrder <= intEmpIdFrom).ToList();                        
                        oApplication.StatusBar.SetText("Searching criteria is not valid for selected range.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return;

                    }
                    if (sortorderto >= sortorderfrom)
                    {
                        Data = Data.Where(e => e.SortOrder >= sortorderfrom && e.SortOrder <= sortorderto).ToList();
                    }
                }
                DIHRMS.Custom.DataServices ds = new DIHRMS.Custom.DataServices(dbHrPayroll, Program.objHrmsUI.HRMSDbName, oCompany.UserName, Program.objHrmsUI.logger);
                string strSql = "SELECT     EmpID, SBOEmpCode, ID, FirstName + ' ' + ISNULL(MiddleName, '') AS empName ,  DepartmentName, LocationName FROM         " + Program.objHrmsUI.HRMSDbName + ".dbo.MstEmployee where ResignDate IS NULL AND ISNULL(flgActive,'1') = 1 ";
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
                foreach (DataRow dr in dtEmp.Rows)
                {
                    emp = (from p in dbHrPayroll.MstEmployee where p.EmpID == dr["EmpID"].ToString() select p).FirstOrDefault();
                    empDetail.Rows.Add(1);

                    empDetail.SetValue("id", i, dr["EmpID"].ToString());
                    empDetail.SetValue("Name", i, dr["empName"].ToString());
                    //if (cbToLocation.Value != "") empDetail.SetValue("toloc", i, cbToLocation.Selected.Description.ToString());
                    empDetail.SetValue("exloc", i, emp.LocationName.ToString());
                    //empDetail.SetValue("Cost", i, emp.CostCenter.ToString());
                    //fillExistlocation(ExLocationID, emp.EmpID);

                    //FillToLocation(LocationID);
                    i++;
                }

                mtEmps.LoadFromDataSource();

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void ApplyLocation()
        {
            try
            {
                DIHRMS.Custom.DataServices ds = new DIHRMS.Custom.DataServices(dbHrPayroll, Program.objHrmsUI.HRMSDbName, oCompany.UserName, Program.objHrmsUI.logger);
                string strSql = "SELECT     EmpID, SBOEmpCode, ID, FirstName + ' ' + ISNULL(MiddleName, '') AS empName ,  DepartmentName, LocationName FROM         " + Program.objHrmsUI.HRMSDbName + ".dbo.MstEmployee where ResignDate IS NULL AND ISNULL(flgActive,'1') = 1 ";
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
                foreach (DataRow dr in dtEmp.Rows)
                {
                    emp = (from p in dbHrPayroll.MstEmployee where p.EmpID == dr["EmpID"].ToString() select p).FirstOrDefault();
                    empDetail.Rows.Add(1);

                    empDetail.SetValue("id", i, dr["EmpID"].ToString());
                    empDetail.SetValue("Name", i, dr["empName"].ToString());

                    //if (cbToLocation.Value != "") empDetail.SetValue("toloc", i, cbToLocation.Selected.Description.ToString());
                    if (cbToLocation.Value != "") empDetail.SetValue("toloc", i, cbToLocation.Value.ToString());

                    FillToLocation(LocationID);

                    empDetail.SetValue("exloc", i, emp.LocationName.ToString());
                    if (chkCostCentre.Checked == true)
                    {
                        if (cbToLocation.Value != "") empDetail.SetValue("cc", i, cbToLocation.Selected.Description.ToString());
                        //FillToLocation(CostCentreID);
                        //empDetail.SetValue("cc", i, emp.LocationName.ToString());

                    }
                    if (chkDimensions.Checked == true)
                    {
                        if (cbDimension1.Value != "") empDetail.SetValue("Dim1", i, cbDimension1.Selected.Description.ToString());
                        if (cbDimension2.Value != "") empDetail.SetValue("Dim2", i, cbDimension2.Selected.Description.ToString());
                        if (cbDimension3.Value != "") empDetail.SetValue("Dim3", i, cbDimension3.Selected.Description.ToString());
                        if (cbDimension4.Value != "") empDetail.SetValue("Dim4", i, cbDimension4.Selected.Description.ToString());
                        if (cbDimension5.Value != "") empDetail.SetValue("Dim5", i, cbDimension5.Selected.Description.ToString());
                    }
                    i++;
                }

                mtEmps.LoadFromDataSource();

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
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

        private void _fillFields()
        {
            oForm.Freeze(true);

            try
            {
                if (currentRecord >= 0)
                {

                    TrnsEmployeeTransfer record;

                    record = empTransfer.ElementAt<TrnsEmployeeTransfer>(currentRecord);
                    txtDocNum.Value = record.DoNum.ToString();
                    if (record.DocDate != null)
                    {
                        if (record.DocDate > DateTime.MinValue)
                        {
                            txtDocDate.Value = Convert.ToDateTime(record.DocDate).ToString("yyyyMMdd");
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
                    //txtStatus.Value = record.StatusRec.ToString();
                    //cbStatus.Value = txtStatus.Value;
                    oForm.DataSources.UserDataSources.Item("cbStatus").ValueEx = record.StatusRec.ToString();

                    empDetail.Rows.Clear();
                    int i = 0;
                    foreach (TrnsEmployeeTransferDetails locDetail in record.TrnsEmployeeTransferDetails)
                    {

                        MstEmployee emp = (from p in dbHrPayroll.MstEmployee where p.ID == locDetail.EmpID select p).FirstOrDefault();
                        MstLocation fromLoc = (from loc in dbHrPayroll.MstLocation where loc.Id == Convert.ToInt32(locDetail.ExistingLocation) select loc).FirstOrDefault();
                        MstLocation empLoc = (from loc in dbHrPayroll.MstLocation where loc.Id == Convert.ToInt32(locDetail.ToLocation) select loc).FirstOrDefault();
                        decimal idd = Convert.ToDecimal(locDetail.EmpID);
                        empDetail.Rows.Add(1);
                        empDetail.SetValue("id", i, emp.EmpID.ToString());
                        empDetail.SetValue("Name", i, locDetail.EmpName.ToString());
                        //empDetail.SetValue("exloc", i, emp.LocationName.ToString());
                        empDetail.SetValue("exloc", i, fromLoc.Name.ToString());
                        empDetail.SetValue("toloc", i, empLoc.Id.ToString());
                        FillToLocation(LocationID);
                        empDetail.SetValue("cc", i, locDetail.CostCentre.ToString());
                        empDetail.SetValue("Dim1", i, locDetail.Dimension1.ToString());
                        empDetail.SetValue("Dim2", i, locDetail.Dimension2.ToString());
                        empDetail.SetValue("Dim3", i, locDetail.Dimension3.ToString());
                        empDetail.SetValue("Dim4", i, locDetail.Dimension4.ToString());
                        empDetail.SetValue("Dim5", i, locDetail.Dimension5.ToString());
                        //FillToLocation(LocationID);

                        i++;
                    }
                    mtEmps.LoadFromDataSource();
                    if (record.StatusRec.ToString() == "0")
                    {
                        IbtnSaveLocation.Enabled = true;
                        IbtnAdd.Enabled = true;
                        btnAdd.Caption = "Update";
                        //IcbStatus.Enabled = true;
                        //oForm.Items.Item("1").Enabled = true;
                        //oForm.Items.Item("40").Enabled = true;
                        oForm.Items.Item("btGetEmp").Enabled = true;

                        //oForm.Items.Item("btCalc").Enabled = true;
                        //oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;

                    }
                    else
                    {
                        //IcbStatus.Enabled = true;
                        oForm.Items.Item("1").Enabled = false;
                        //oForm.Items.Item("40").Enabled = false;
                        oForm.Items.Item("btGetEmp").Enabled = false;
                        oForm.Items.Item("btAppLoc").Enabled = false;
                        //oForm.Items.Item("btCalc").Enabled = false;
                        oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                        IbtnSaveLocation.Enabled = false;
                        IbtnAdd.Enabled = false;
                        IcbStatus.Enabled = false;

                    }


                }
            }
            catch (Exception ex)
            {
                oForm.Freeze(false);
            }

            oForm.Freeze(false);
        }

        private void AddValidValuesInCombos()
        {            
            cbDimension1.ValidValues.Add("-1", "Select Distribution");
            cbDimension2.ValidValues.Add("-1", "Select Distribution");
            cbDimension3.ValidValues.Add("-1", "Select Distribution");
            cbDimension4.ValidValues.Add("-1", "Select Distribution");
            cbDimension5.ValidValues.Add("-1", "Select Distribution");
        }

        private void FillComboDimension1(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                string strValueName, strValueActive;
                string strSql = "SELECT \"DimDesc\" FROM \"ODIM\" WHERE \"DimCode\" = 1";
                string strSql1 = "SELECT \"DimActive\" FROM \"ODIM\" WHERE \"DimCode\" = 1";
                SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecSet.DoQuery(strSql);
                strValueName = Convert.ToString(oRecSet.Fields.Item("DimDesc").Value);
                if (!string.IsNullOrEmpty(strValueName))
                {
                    lblDimension1.Caption = strValueName;
                }
                oRecSet.DoQuery(strSql1);
                strValueActive = Convert.ToString(oRecSet.Fields.Item("DimActive").Value);
                if (!string.IsNullOrEmpty(strValueActive) && strValueActive == "Y")
                {
                    flgDim1 = true;
                }
                else
                {
                    flgDim1 = false;
                }
                List<string> ccCode = new List<string>();
                List<string> ccName = new List<string>();
                string strSql3 = "SELECT \"OcrCode\", \"OcrName\" FROM \"OOCR\" WHERE \"DimCode\" = '1' and \"Active\"='Y'";
                SAPbobsCOM.Recordset oRecSet1 = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecSet1.DoQuery(strSql3);
                while (oRecSet1.EoF == false)
                {
                    if (!ccCode.Contains(Convert.ToString(oRecSet1.Fields.Item("OcrCode").Value)))
                    {
                        ccCode.Add(Convert.ToString(oRecSet1.Fields.Item("OcrCode").Value).Trim());
                        ccName.Add(Convert.ToString(oRecSet1.Fields.Item("OcrName").Value).Trim());
                    }
                    oRecSet1.MoveNext();
                }
                //pCombo.ValidValues.Add("-1", "Select Distribution");
                for (Int32 i = 0; i < ccCode.Count; i++)
                {
                    pCombo.ValidValues.Add(ccCode[i].ToString().Trim(), ccName[i].ToString().Trim());
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("d1 : " + Ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }


        }

        private void FillComboDimension2(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                string strValueName, strValueActive;
                string strSql = "SELECT \"DimDesc\" FROM \"ODIM\" WHERE \"DimCode\" = 2";
                string strSql1 = "SELECT \"DimActive\" FROM \"ODIM\" WHERE \"DimCode\" = 2";
                SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecSet.DoQuery(strSql);
                strValueName = Convert.ToString(oRecSet.Fields.Item("DimDesc").Value);
                if (!string.IsNullOrEmpty(strValueName))
                {
                    lblDimension2.Caption = strValueName;
                }
                oRecSet.DoQuery(strSql1);
                strValueActive = Convert.ToString(oRecSet.Fields.Item("DimActive").Value);
                if (!string.IsNullOrEmpty(strValueActive) && strValueActive == "Y")
                {
                    flgDim2 = true;
                }
                else
                {
                    flgDim2 = false;
                }
                List<string> ccCode = new List<string>();
                List<string> ccName = new List<string>();
                string strSql3 = "SELECT \"OcrCode\", \"OcrName\" FROM \"OOCR\" WHERE \"DimCode\" = '2' AND \"Active\"='Y'";
                SAPbobsCOM.Recordset oRecSet1 = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecSet1.DoQuery(strSql3);
                while (oRecSet1.EoF == false)
                {

                    if (!ccCode.Contains(Convert.ToString(oRecSet1.Fields.Item("OcrCode").Value)))
                    {
                        ccCode.Add(Convert.ToString(oRecSet1.Fields.Item("OcrCode").Value));
                        ccName.Add(Convert.ToString(oRecSet1.Fields.Item("OcrName").Value));
                    }
                    oRecSet1.MoveNext();
                }
                //pCombo.ValidValues.Add("-1", "Select Distribution");
                for (Int32 i = 0; i < ccCode.Count; i++)
                {
                    pCombo.ValidValues.Add(ccCode[i], ccName[i]);
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("d1 : " + Ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }


        }

        private void FillComboDimension3(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                string strValueName, strValueActive;
                string strSql = "SELECT \"DimDesc\" FROM \"ODIM\" WHERE \"DimCode\" = 3";
                string strSql1 = "SELECT \"DimActive\" FROM \"ODIM\" WHERE \"DimCode\" = 3";
                SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecSet.DoQuery(strSql);
                strValueName = Convert.ToString(oRecSet.Fields.Item("DimDesc").Value);
                if (!string.IsNullOrEmpty(strValueName))
                {
                    lblDimension3.Caption = strValueName;
                }
                oRecSet.DoQuery(strSql1);
                strValueActive = Convert.ToString(oRecSet.Fields.Item("DimActive").Value);
                if (!string.IsNullOrEmpty(strValueActive) && strValueActive == "Y")
                {
                    flgDim3 = true;
                }
                else
                {
                    flgDim3 = false;
                }
                List<string> ccCode = new List<string>();
                List<string> ccName = new List<string>();
                string strSql3 = "SELECT \"OcrCode\", \"OcrName\" FROM \"OOCR\" WHERE \"DimCode\" = '3' AND \"Active\"='Y'";
                SAPbobsCOM.Recordset oRecSet1 = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecSet1.DoQuery(strSql3);
                while (oRecSet1.EoF == false)
                {

                    if (!ccCode.Contains(Convert.ToString(oRecSet1.Fields.Item("OcrCode").Value)))
                    {
                        ccCode.Add(Convert.ToString(oRecSet1.Fields.Item("OcrCode").Value));
                        ccName.Add(Convert.ToString(oRecSet1.Fields.Item("OcrName").Value));
                    }
                    oRecSet1.MoveNext();
                }
                //pCombo.ValidValues.Add("-1", "Select Distribution");
                for (Int32 i = 0; i < ccCode.Count; i++)
                {
                    pCombo.ValidValues.Add(ccCode[i], ccName[i]);
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("d1 : " + Ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }


        }

        private void FillComboDimension4(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                string strValueName, strValueActive;
                string strSql = "SELECT \"DimDesc\" FROM \"ODIM\" WHERE \"DimCode\" = 4";
                string strSql1 = "SELECT \"DimActive\" FROM \"ODIM\" WHERE \"DimCode\" = 4";
                SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecSet.DoQuery(strSql);
                strValueName = Convert.ToString(oRecSet.Fields.Item("DimDesc").Value);
                if (!string.IsNullOrEmpty(strValueName))
                {
                    lblDimension4.Caption = strValueName;
                }
                oRecSet.DoQuery(strSql1);
                strValueActive = Convert.ToString(oRecSet.Fields.Item("DimActive").Value);
                if (!string.IsNullOrEmpty(strValueActive) && strValueActive == "Y")
                {
                    flgDim4 = true;
                }
                else
                {
                    flgDim4 = false;
                }
                List<string> ccCode = new List<string>();
                List<string> ccName = new List<string>();
                string strSql3 = "SELECT \"OcrCode\", \"OcrName\" FROM \"OOCR\" WHERE \"DimCode\" = '4' AND \"Active\"='Y'";
                SAPbobsCOM.Recordset oRecSet1 = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecSet1.DoQuery(strSql3);
                while (oRecSet1.EoF == false)
                {

                    if (!ccCode.Contains(Convert.ToString(oRecSet1.Fields.Item("OcrCode").Value)))
                    {
                        ccCode.Add(Convert.ToString(oRecSet1.Fields.Item("OcrCode").Value));
                        ccName.Add(Convert.ToString(oRecSet1.Fields.Item("OcrName").Value));
                    }
                    oRecSet1.MoveNext();
                }
                //pCombo.ValidValues.Add("-1", "Select Distribution");
                for (Int32 i = 0; i < ccCode.Count; i++)
                {
                    pCombo.ValidValues.Add(ccCode[i], ccName[i]);
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("d1 : " + Ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }


        }

        private void FillComboDimension5(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                string strValueName, strValueActive;
                string strSql = "SELECT \"DimDesc\" FROM \"ODIM\" WHERE \"DimCode\" = 5";
                string strSql1 = "SELECT \"DimActive\" FROM \"ODIM\" WHERE \"DimCode\" = 5";
                SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecSet.DoQuery(strSql);
                strValueName = Convert.ToString(oRecSet.Fields.Item("DimDesc").Value);
                if (!string.IsNullOrEmpty(strValueName))
                {
                    lblDimension5.Caption = strValueName;
                }
                oRecSet.DoQuery(strSql1);
                strValueActive = Convert.ToString(oRecSet.Fields.Item("DimActive").Value);
                if (!string.IsNullOrEmpty(strValueActive) && strValueActive == "Y")
                {
                    flgDim5 = true;
                }
                else
                {
                    flgDim5 = false;
                }
                List<string> ccCode = new List<string>();
                List<string> ccName = new List<string>();
                string strSql3 = "SELECT \"OcrCode\", \"OcrName\" FROM \"OOCR\" WHERE \"DimCode\" = '5' AND \"Active\"='Y'";
                SAPbobsCOM.Recordset oRecSet1 = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecSet1.DoQuery(strSql3);
                while (oRecSet1.EoF == false)
                {

                    if (!ccCode.Contains(Convert.ToString(oRecSet1.Fields.Item("OcrCode").Value)))
                    {
                        ccCode.Add(Convert.ToString(oRecSet1.Fields.Item("OcrCode").Value));
                        ccName.Add(Convert.ToString(oRecSet1.Fields.Item("OcrName").Value));
                    }
                    oRecSet1.MoveNext();
                }
                //pCombo.ValidValues.Add("-1", "Select Distribution");
                for (Int32 i = 0; i < ccCode.Count; i++)
                {
                    pCombo.ValidValues.Add(ccCode[i], ccName[i]);
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("d1 : " + Ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }


        }
        #endregion

    }
}
