
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using DIHRMS.Custom;


namespace ACHR.Screen
{
    class frm_BHcount : HRMSBaseForm
    {
        
        #region "Global Variable"
        
        SAPbouiCOM.EditText txtDocumentNumber, txtFromDate, txtToDate, txtDescription;
        SAPbouiCOM.ComboBox cbLocation;
        SAPbouiCOM.Matrix mtMain;
        SAPbouiCOM.Column Serial, IsNew, Id, Designation, Branch, Department, BudgetHeadCount, OccupiedPosition, RemainingVacancies;
        SAPbouiCOM.Button btnMain, btnCancel;
        SAPbouiCOM.DataTable dtMain;
        SAPbouiCOM.Item ibtnMain, ibtnCancel;
        SAPbouiCOM.Item itxtDocumentNumber, itxtDescription, itxtFromDate, itxtToDate, icbLocation, imtMain;

        IEnumerable<TrnsHeadBudget> oDocuments = null;

        #endregion

        #region "B1 Form Events"
        
        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.Freeze(true);
            InitiallizeForm();
            oForm.Freeze(false);
        }

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "btmain":
                    CheckState();
                    break;
                case "btnew":
                    break;
                case "btprev":
                    break;
                case "btforw":
                    break;
            }
            
        }

        public override void etAfterLostFocus(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterLostFocus(ref pVal, ref BubbleEvent);

            if (pVal.ItemUID == "mtmain" && pVal.ColUID == "bhc")
            {
                oForm.Freeze(true);
                mtMain.FlushToDataSource();
                AddEmptyRow();
                oForm.Freeze(false);
            }
            if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_UPDATE_MODE)
            {
                if (btnMain.Caption == "Ok")
                {
                    btnMain.Caption = "Update";
                }
            }
        }

        public override void getNextRecord()
        {
            base.getNextRecord();
            //GetNextRecord();
        }

        public override void getPreviouRecord()
        {
            base.getPreviouRecord();
            //GetPreviosRecord();
        }

        public override void getFirstRecord()
        {
            base.getFirstRecord();
            //CurrentRecord = 0;
            //FillDocument(CurrentRecord);
        }

        public override void getLastRecord()
        {
            base.getLastRecord();
            //CurrentRecord = TotalRecords - 1;
            //FillDocument(CurrentRecord);
        }

        public override void AddNewRecord()
        {
           base.AddNewRecord();
           oForm.Freeze(true);
           InitiallizeDocument();
           oForm.Freeze(false);
            
        }

        public override void fillFields()
        {
            base.fillFields();
            oForm.Freeze(true);
            FillDocument();
            oForm.Freeze(false);
        }

        #endregion

        #region "Local Methods"
        
        private void InitiallizeForm()
        {
            try
            {
                btnMain = oForm.Items.Item("btmain").Specific;
                ibtnMain = oForm.Items.Item("btmain");
                btnCancel = oForm.Items.Item("2").Specific;
                ibtnCancel = oForm.Items.Item("2");

                txtDocumentNumber = oForm.Items.Item("txdocno").Specific;
                itxtDocumentNumber = oForm.Items.Item("txdocno");
                oForm.DataSources.UserDataSources.Add("txdocno", SAPbouiCOM.BoDataType.dt_LONG_NUMBER, 10);
                txtDocumentNumber.DataBind.SetBound(true, "", "txdocno");
                txtDocumentNumber.Value = Convert.ToString(ds.GetDocumentNumber(-1,14));

                txtDescription = oForm.Items.Item("txdesc").Specific;
                itxtDescription = oForm.Items.Item("txdesc");
                oForm.DataSources.UserDataSources.Add("txdesc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txtDescription.DataBind.SetBound(true, "", "txdesc");

                txtFromDate = oForm.Items.Item("txfrom").Specific;
                itxtFromDate = oForm.Items.Item("txfrom");
                oForm.DataSources.UserDataSources.Add("txfrom", SAPbouiCOM.BoDataType.dt_DATE, 30);
                txtFromDate.DataBind.SetBound(true, "", "txfrom");

                txtToDate = oForm.Items.Item("txto").Specific;
                itxtToDate = oForm.Items.Item("txto");
                oForm.DataSources.UserDataSources.Add("txto", SAPbouiCOM.BoDataType.dt_DATE, 30);
                txtToDate.DataBind.SetBound(true, "", "txto");

                cbLocation = oForm.Items.Item("cbloc").Specific;
                icbLocation = oForm.Items.Item("cbloc");
                oForm.DataSources.UserDataSources.Add("cbloc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cbLocation.DataBind.SetBound(true, "", "cbloc");
                

                mtMain = oForm.Items.Item("mtmain").Specific;
                imtMain = oForm.Items.Item("mtmain");
                dtMain = oForm.DataSources.DataTables.Item("dtmain");
                IsNew = mtMain.Columns.Item("isnew");
                IsNew.Visible = false;
                Id = mtMain.Columns.Item("id");
                Id.Visible = false;
                Serial = mtMain.Columns.Item("serial");
                Designation = mtMain.Columns.Item("desig");
                Department = mtMain.Columns.Item("dept");
                Branch = mtMain.Columns.Item("branch");
                BudgetHeadCount = mtMain.Columns.Item("bhc");
                OccupiedPosition = mtMain.Columns.Item("occposi");
                RemainingVacancies = mtMain.Columns.Item("remvac");

                FillDepartmentInColumn(Department);
                FillDesignationInColumn(Designation);
                FillBranchInColumn(Branch);
                FillLocations(cbLocation);
                
                GetData();
                InitiallizeDocument();
                FormStatus();
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("InitializeFrom Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FormStatus()
        {
            itxtDocumentNumber.AffectsFormMode = true;
            itxtFromDate.AffectsFormMode = true;
            itxtToDate.AffectsFormMode = true;
            imtMain.AffectsFormMode = true;
        }

        private void SetCurrentYearPeriod()
        {
            try
            {
                DateTime TodayDate = DateTime.Now;
                MstCalendar CurrentPeriod = (from a in dbHrPayroll.MstCalendar
                                             where a.StartDate <= TodayDate &&
                                                   a.EndDate >= TodayDate
                                             select a).FirstOrDefault();
                if (CurrentPeriod != null)
                {
                    txtFromDate.Value = Convert.ToDateTime(CurrentPeriod.StartDate).ToString("yyyyMMdd");
                    txtToDate.Value = Convert.ToDateTime(CurrentPeriod.EndDate).ToString("yyyyMMdd");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SetCurrentYearPeriod Function Error : " + ex.Message);
            }
        }

        private void FillLocations(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                IEnumerable<MstLocation> Locations = from a in dbHrPayroll.MstLocation select a;
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

        private void AddEmptyRow()
        {
            Int32 RowValue = 0;
            if (dtMain.Rows.Count == 0)
            {
                dtMain.Rows.Add(1);
                RowValue = dtMain.Rows.Count;
                dtMain.SetValue(IsNew.DataBind.Alias, RowValue - 1, "Y");
                dtMain.SetValue(Id.DataBind.Alias, RowValue - 1, "0");
                dtMain.SetValue(Designation.DataBind.Alias, RowValue - 1, "");
                dtMain.SetValue(Department.DataBind.Alias, RowValue - 1, "");
                dtMain.SetValue(Branch.DataBind.Alias, RowValue - 1, "");
                dtMain.SetValue(BudgetHeadCount.DataBind.Alias, RowValue - 1, "0");
                dtMain.SetValue(OccupiedPosition.DataBind.Alias, RowValue - 1, "0");
                dtMain.SetValue(RemainingVacancies.DataBind.Alias, RowValue - 1, "0");
                //dtMain.SetValue(Serial.DataBind.Alias, RowValue - 1, RowValue-1);
                mtMain.AddRow(1, 0);
            }
            else
            {
                if (dtMain.GetValue(Designation.DataBind.Alias, dtMain.Rows.Count - 1) == "")
                {
                }
                else
                {
                    
                    dtMain.Rows.Add(1);
                    RowValue = dtMain.Rows.Count;
                    dtMain.SetValue(IsNew.DataBind.Alias, RowValue - 1, "Y");
                    dtMain.SetValue(Id.DataBind.Alias, RowValue - 1, "0");
                    dtMain.SetValue(Designation.DataBind.Alias, RowValue - 1, "");
                    dtMain.SetValue(Department.DataBind.Alias, RowValue - 1, "");
                    dtMain.SetValue(Branch.DataBind.Alias, RowValue - 1, "");
                    dtMain.SetValue(BudgetHeadCount.DataBind.Alias, RowValue - 1, "0");
                    dtMain.SetValue(OccupiedPosition.DataBind.Alias, RowValue - 1, "0");
                    dtMain.SetValue(RemainingVacancies.DataBind.Alias, RowValue - 1, "0");
                    //dtMain.SetValue(Serial.DataBind.Alias, RowValue - 1, RowValue-1);
                    mtMain.AddRow(1, mtMain.RowCount);
                }
            }
            mtMain.LoadFromDataSource();
        }

        private void FillDocument()
        {
            try
            {
                dtMain.Rows.Clear();
                mtMain.LoadFromDataSource();
                TrnsHeadBudget oDoc = oDocuments.ElementAt<TrnsHeadBudget>(currentRecord);

                cbLocation.Select(oDoc.Location.ToString(), SAPbouiCOM.BoSearchKey.psk_ByValue);
                txtDocumentNumber.Value = oDoc.DocNum.ToString();
                txtDescription.Value = oDoc.Description;
                txtFromDate.Value = Convert.ToDateTime(oDoc.FromDt).ToString("yyyyMMdd");
                txtToDate.Value = Convert.ToDateTime(oDoc.ToDt).ToString("yyyyMMdd");
                Int32 i = 0;
                foreach (TrnsHeadBudgetDetail OneLine in oDoc.TrnsHeadBudgetDetail)
                {
                    dtMain.Rows.Add(1);
                    Int32 GetOccupied = 0, Remaining =0, CheckOccupied;
                    CheckOccupied = (from a in dbHrPayroll.TrnsJobRequisition
                                                   where a.BaseDoc == oDoc.DocNum &&
                                                         a.MstLocation.Name.Contains(oDoc.MstLocation.Name) &&
                                                         a.MstBranches.Name.Contains(OneLine.MstBranches.Name) &&
                                                         a.MstDesignation.Name.Contains(OneLine.MstDesignation.Name) &&
                                                         a.MstDepartment.DeptName.Contains(OneLine.MstDepartment.DeptName)
                                                   select a).Count();
                    if (CheckOccupied > 0)
                    {
                        GetOccupied = Convert.ToInt32((from a in dbHrPayroll.TrnsJobRequisition
                                                       where a.BaseDoc == oDoc.DocNum &&
                                                             a.MstLocation.Name.Contains(oDoc.MstLocation.Name) &&
                                                             a.MstBranches.Name.Contains(OneLine.MstBranches.Name) &&
                                                             a.MstDesignation.Name.Contains(OneLine.MstDesignation.Name) &&
                                                             a.MstDepartment.DeptName.Contains(OneLine.MstDepartment.DeptName)
                                                       select a).Count());
                    }
                    //dtMain.SetValue(Serial.DataBind.Alias, i, i);
                    dtMain.SetValue(IsNew.DataBind.Alias, i, "N");
                    dtMain.SetValue(Id.DataBind.Alias, i, OneLine.ID);
                    dtMain.SetValue(Designation.DataBind.Alias, i, OneLine.DesignationID);
                    dtMain.SetValue(Branch.DataBind.Alias, i, OneLine.BranchID);
                    dtMain.SetValue(Department.DataBind.Alias, i, OneLine.DepartmentID);
                    dtMain.SetValue(BudgetHeadCount.DataBind.Alias, i, OneLine.BudgetedHearcount);
                    dtMain.SetValue(OccupiedPosition.DataBind.Alias, i, GetOccupied);
                    Remaining = (Int32)OneLine.BudgetedHearcount - GetOccupied;
                    dtMain.SetValue(RemainingVacancies.DataBind.Alias, i, Remaining);
                    i++;
                }
                AddEmptyRow();
                btnMain.Caption = "Ok";
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Exception FillDocument Error : "+ Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void InitiallizeDocument()
        {
            try
            {
                btnMain.Caption = "Add";
                txtDocumentNumber.Value = Convert.ToString(ds.GetDocumentNumber(-1,14));
                cbLocation.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                itxtToDate.Click();
                itxtDocumentNumber.Enabled = false;
                txtFromDate.Value = "";
                txtToDate.Value = "";
                dtMain.Rows.Clear();
                AddEmptyRow();
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void FillDesignationInColumn(SAPbouiCOM.Column OneColumn)
        {
            try
            {
                IEnumerable<MstDesignation> Designations = from a in dbHrPayroll.MstDesignation select a;
                OneColumn.ValidValues.Add("-1", "");
                foreach (MstDesignation Designation in Designations)
                {
                    OneColumn.ValidValues.Add(Convert.ToString(Designation.Id), Convert.ToString(Designation.Name));
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void FillDepartmentInColumn(SAPbouiCOM.Column OneColumn)
        {
            try
            {
                IEnumerable<MstDepartment> Departments = from a in dbHrPayroll.MstDepartment select a;
                OneColumn.ValidValues.Add("-1", "");
                foreach(MstDepartment Department in Departments)
                {
                    OneColumn.ValidValues.Add(Convert.ToString(Department.ID), Convert.ToString(Department.DeptName));
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void FillBranchInColumn(SAPbouiCOM.Column OneColumn)
        {
            try
            {
                IEnumerable<MstBranches> Branches = from a in dbHrPayroll.MstBranches select a;
                OneColumn.ValidValues.Add("-1", "");
                foreach (MstBranches Branch in Branches)
                {
                    OneColumn.ValidValues.Add(Convert.ToString(Branch.Id), Convert.ToString(Branch.Name));
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void CheckState()
        {
            try
            {
                if (btnMain.Caption == "Add")
                {
                    if (ValidateForm())
                    {
                        if (AddDocument())
                        {
                            InitiallizeDocument();
                        }
                        else
                        {
                            btnMain.Caption = "Add";
                        }
                        return;
                    }
                }
                if (btnMain.Caption == "Update")
                {
                    if (ValidateForm())
                    {
                        if (UpdateDocument())
                        {
                            InitiallizeDocument();
                        }
                        else
                        {
                            btnMain.Caption = "Update";
                        }
                        return;
                    }
                }
                if (btnMain.Caption == "Ok")
                {
                    oForm.Close();
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private Boolean AddDocument()
        {
            Boolean retValue = true;
            try
            {
                dtMain.Rows.Clear();
                mtMain.FlushToDataSource();
                TrnsHeadBudget oDoc = new TrnsHeadBudget();
                oDoc.Location = Convert.ToInt32(cbLocation.Value);
                oDoc.LocationName = (from a in dbHrPayroll.MstLocation where a.Id == Convert.ToInt32(cbLocation.Value) select a.Name).FirstOrDefault();
                oDoc.FromDt = DateTime.ParseExact(txtFromDate.Value,"yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                oDoc.ToDt = DateTime.ParseExact(txtToDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                oDoc.Description = txtDescription.Value.Trim();
                oDoc.Series = -1;
                oDoc.DocType = 14;
                oDoc.DocNum = Convert.ToInt32(txtDocumentNumber.Value);
                for (Int16 i = 0; i < dtMain.Rows.Count; i++)
                {
                    if (String.IsNullOrEmpty(dtMain.GetValue(Designation.DataBind.Alias, i)))
                    {
                        continue;
                    }
                    else
                    {

                        Int32 DesigID, DeptID, BranchID, HeadCount, OccVac;
                        TrnsHeadBudgetDetail oDetail = new TrnsHeadBudgetDetail();
                        DesigID = Convert.ToInt32(dtMain.GetValue(Designation.DataBind.Alias, i));
                        DeptID = Convert.ToInt32(dtMain.GetValue(Department.DataBind.Alias, i));
                        BranchID = Convert.ToInt32(dtMain.GetValue(Branch.DataBind.Alias, i));
                        HeadCount = Convert.ToInt16(dtMain.GetValue(BudgetHeadCount.DataBind.Alias, i));
                        OccVac = Convert.ToInt16(dtMain.GetValue(OccupiedPosition.DataBind.Alias, i));

                        oDetail.DesignationID = DesigID;
                        oDetail.Designation = (from a in dbHrPayroll.MstDesignation where a.Id == DesigID select a.Name).FirstOrDefault();
                        oDetail.DepartmentID = DeptID;
                        oDetail.Department = (from a in dbHrPayroll.MstDepartment where a.ID == DeptID select a.DeptName).FirstOrDefault();
                        oDetail.BranchID = BranchID;
                        oDetail.Branch = (from a in dbHrPayroll.MstBranches where a.Id == BranchID select a.Name).FirstOrDefault();
                        oDetail.BudgetedHearcount = Convert.ToInt16(HeadCount);
                        oDetail.OccupiedPositions = Convert.ToInt16(OccVac);
                        oDoc.TrnsHeadBudgetDetail.Add(oDetail);
                    }
                }
                dbHrPayroll.TrnsHeadBudget.InsertOnSubmit(oDoc);
                oDoc.UserId = oCompany.UserName;
                oDoc.CreateDate = DateTime.Now;
                oDoc.UpdatedBy = oCompany.UserName;
                oDoc.UpdateDate = DateTime.Now;
                dbHrPayroll.SubmitChanges();
                btnMain.Caption = "Ok";
                oApplication.StatusBar.SetText("Document Added Successfully", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Exception AddDocument Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                retValue = false;
            }
            return retValue;
        }

        private Boolean UpdateDocument()
        {
            Boolean retValue = true;
            try
            {
                Int32 HeadCount, OccPosition, DocumentNumber, id;
                String isnew, DesigId, BranchId, DeptId;
                dtMain.Rows.Clear();
                mtMain.FlushToDataSource();
                DocumentNumber = Convert.ToUInt16(txtDocumentNumber.Value);
                TrnsHeadBudget oDoc = null;
                oDoc = (from a in dbHrPayroll.TrnsHeadBudget where a.DocNum == DocumentNumber select a).FirstOrDefault();
                if (oDoc != null)
                {
                    oDoc.Location = Convert.ToInt32(cbLocation.Value);
                    oDoc.LocationName = (from a in dbHrPayroll.MstLocation where a.Id == Convert.ToInt32(cbLocation.Value) select a.Name).FirstOrDefault();
                    oDoc.FromDt = DateTime.ParseExact(txtFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    oDoc.ToDt = DateTime.ParseExact(txtToDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    oDoc.Description = txtDescription.Value.Trim();
                    oDoc.Series = -1;
                    oDoc.DocType = 14;
                    for (Int32 i = 0; i < dtMain.Rows.Count; i++)
                    {

                        id = dtMain.GetValue(Id.DataBind.Alias, i);
                        isnew = dtMain.GetValue(IsNew.DataBind.Alias, i);
                        DesigId = dtMain.GetValue(Designation.DataBind.Alias, i);
                        DeptId = dtMain.GetValue(Department.DataBind.Alias, i);
                        BranchId = dtMain.GetValue(Branch.DataBind.Alias, i);
                        HeadCount = Convert.ToInt32(dtMain.GetValue(BudgetHeadCount.DataBind.Alias, i));
                        OccPosition = Convert.ToInt32(dtMain.GetValue(OccupiedPosition.DataBind.Alias, i));
                        if (!String.IsNullOrEmpty(DeptId))
                        {
                            if (isnew == "N")
                            {
                                TrnsHeadBudgetDetail oDetail = (from a in dbHrPayroll.TrnsHeadBudgetDetail where a.ID == id select a).FirstOrDefault();
                                MstDesignation oDesig = (from a in dbHrPayroll.MstDesignation where a.Id == Convert.ToInt32(DesigId) select a).FirstOrDefault();
                                oDetail.MstDesignation = oDesig;
                                oDetail.Designation = (from a in dbHrPayroll.MstDesignation where a.Id == Convert.ToInt32(DesigId) select a.Name).FirstOrDefault();
                                MstDepartment oDept = (from a in dbHrPayroll.MstDepartment where a.ID == Convert.ToInt32(DeptId) select a).FirstOrDefault();
                                oDetail.MstDepartment = oDept;
                                oDetail.Department = (from a in dbHrPayroll.MstDepartment where a.ID == Convert.ToInt32(DeptId) select a.DeptName).FirstOrDefault();
                                MstBranches oBranch = (from a in dbHrPayroll.MstBranches where a.Id == Convert.ToInt32(BranchId) select a).FirstOrDefault();
                                oDetail.MstBranches = oBranch;
                                oDetail.Branch = (from a in dbHrPayroll.MstBranches where a.Id == Convert.ToInt32(BranchId) select a.Name).FirstOrDefault();
                                oDetail.BudgetedHearcount = Convert.ToInt16(HeadCount);
                                oDetail.OccupiedPositions = Convert.ToInt16(OccPosition);
                            }
                            else
                            {
                                TrnsHeadBudgetDetail oDetail = new TrnsHeadBudgetDetail();
                                oDetail.DesignationID = Convert.ToInt32(DesigId);
                                oDetail.Designation = (from a in dbHrPayroll.MstDesignation where a.Id == Convert.ToInt32(DesigId) select a.Name).FirstOrDefault();
                                oDetail.DepartmentID = Convert.ToInt32(DeptId);
                                oDetail.Department = (from a in dbHrPayroll.MstDepartment where a.ID == Convert.ToInt32(DeptId) select a.DeptName).FirstOrDefault();
                                oDetail.BranchID = Convert.ToInt32(BranchId);
                                oDetail.Branch = (from a in dbHrPayroll.MstBranches where a.Id == Convert.ToInt32(BranchId) select a.Name).FirstOrDefault();
                                oDetail.BudgetedHearcount = Convert.ToInt16(HeadCount);
                                oDetail.OccupiedPositions = Convert.ToInt16(OccPosition);
                                oDoc.TrnsHeadBudgetDetail.Add(oDetail);
                            }
                        }
                    }

                    
                    oDoc.UpdateDate = DateTime.Now;
                    oDoc.UpdatedBy = oCompany.UserName;
                    dbHrPayroll.SubmitChanges();
                    oApplication.StatusBar.SetText("Document Updated Successfully", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                    
                }
                else
                {
                    retValue = false;
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Exception UpdateDocument Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                retValue = false;
            }
            return retValue;
        }

        private Boolean ValidateForm()
        {
            Boolean retValue = true;
            DateTime FromDate, ToDate;
            Int32 CountDoc = 0, curDocNum = 0, LocationID =0;
            //
            try
            {
                //Check Dates
                curDocNum = Convert.ToInt32(txtDocumentNumber.Value);
                LocationID = Convert.ToInt32(cbLocation.Value.Trim());
                FromDate = DateTime.ParseExact(txtFromDate.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                ToDate = DateTime.ParseExact(txtToDate.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                for (DateTime One = FromDate; One <= ToDate; One = One.AddDays(1))
                {
                    CountDoc += (from a in dbHrPayroll.TrnsHeadBudget
                                  where a.FromDt <= One && a.ToDt >= One && a.DocNum != curDocNum && a.Location == LocationID
                                  select a).Count();

                }
                if (CountDoc > 0)
                {
                    retValue = false;
                    oApplication.StatusBar.SetText("Budget Overlaps From & To Dates", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                }

            }
            catch
            {
                retValue = false;
            }
            return retValue;
        }

        private void GetData()
        {
            CodeIndex.Clear();
            oDocuments = from a in dbHrPayroll.TrnsHeadBudget select a;
            Int32 i = 0;
            foreach (TrnsHeadBudget oDoc in oDocuments)
            {
                CodeIndex.Add(oDoc.ID, oDoc.DocNum);
                i++;
            }
            totalRecord = i;
        }

        #endregion

    }
}
