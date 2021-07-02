using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using DIHRMS.Custom;
using System.Data;

namespace ACHR.Screen
{
    class frm_SLCan:HRMSBaseForm
    {
        #region "Global Variable"

        SAPbouiCOM.EditText txtValidityFrom, txtValidityTo, txtCandidateNoFrom, txtCandidateNoTo, txtVacancyFrom, txtVacancyTo;
        SAPbouiCOM.EditText txtPositionFrom, txtPositionTo, txtDeptFrom, txtDeptTo, txtBranchFrom, txtBranchTo;
        SAPbouiCOM.ComboBox cbStatus, cbPosition, cbDepartment, cbBranch, cbLocation;
        SAPbouiCOM.Button btnMain, btnReleaseToShortList, btnSearch;
        SAPbouiCOM.Matrix mtOpen, mtShortListed;
        SAPbouiCOM.DataTable dtOpen, dtShortListed;
        SAPbouiCOM.Column oSerial, oSelected, oCandidateNo, oFirstName, oLastName, oDept, oPosition, oBranch, oVfrom, oVtill, oCity, oCountry;
        SAPbouiCOM.Column sSerial, sSelected, sCandidateNo, sFirstName, sLastName, sDept, sPosition, sBranch, sVfrom, sVtill, sCity, sCountry;
        SAPbouiCOM.ButtonCombo btnCreate;
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
                    CheckMainButtonState();
                    break;
                case "btcombo":
                    if (btnCreate.Selected.Value == "Employee")
                    {
                        CreateEmployee();
                    }
                    if (btnCreate.Selected.Value == "Interview")
                    {
                        CreateInterviewCall();
                    }
                    break;
                case "btrsl":
                    ReleaseToShortList();
                    break;
                case "btsearch":
                    SearchCandidate();
                    break;
            }
        }

        public override void FindRecordMode()
        {
            base.FindRecordMode();
            oForm.Freeze(true);
            InitiallizeDocument();
            btnMain.Caption = "Find";
            oForm.Freeze(false);
        }

        public override void PrepareSearchKeyHash()
        {
            
            base.PrepareSearchKeyHash();
            SearchKeyVal.Clear();
            try
            {
                SearchKeyVal.Add("Status", cbStatus.Value.Trim());
                SearchKeyVal.Add("CanFrom", txtCandidateNoFrom.Value.Trim() != "" ? txtCandidateNoFrom.Value.Trim() : "0");
                SearchKeyVal.Add("CanTo", txtCandidateNoTo.Value.Trim() != "" ? txtCandidateNoTo.Value.Trim() : "100000000");
                SearchKeyVal.Add("ValidFrom", txtValidityFrom.Value.Trim() != "" ? txtValidityFrom.Value.Trim() : "2001/06/06");
                SearchKeyVal.Add("ValidTo", txtValidityTo.Value.Trim() != "" ? txtValidityTo.Value.Trim() : "2030/06/06");
                SearchKeyVal.Add("JRFrom", txtVacancyFrom.Value.Trim() != "" ? txtVacancyFrom.Value.Trim() : "0");
                SearchKeyVal.Add("JRTo", txtVacancyTo.Value.Trim() != "" ? txtVacancyTo.Value.Trim() : "100000000");
                if (cbDepartment.Selected.Description.Trim() != "-1") 
                    SearchKeyVal.Add("Department", cbDepartment.Selected.Description.Trim());
                else
                    SearchKeyVal.Add("Department", "");
                if (cbLocation.Selected.Description.Trim() != "-1") 
                    SearchKeyVal.Add("Location", cbLocation.Selected.Description.Trim());
                else
                    SearchKeyVal.Add("Location", "");
                if (cbBranch.Selected.Description.Trim() != "-1") 
                    SearchKeyVal.Add("Branches", cbBranch.Selected.Description.Trim());
                else
                    SearchKeyVal.Add("Branches", "");
                if (cbPosition.Selected.Description.Trim() != "-1")
                    SearchKeyVal.Add("Designation", cbPosition.Selected.Description.Trim());
                else
                    SearchKeyVal.Add("Designation", "");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        #endregion

        #region "Local Methods"

        private void InitiallizeForm()
        {
            try
            {
                cbStatus = oForm.Items.Item("cbstatus").Specific;
                oForm.DataSources.UserDataSources.Add("cbstatus", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                cbStatus.DataBind.SetBound(true, "", "cbstatus");

                cbBranch = oForm.Items.Item("cbbranch").Specific;
                oForm.DataSources.UserDataSources.Add("cbbranch", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                cbBranch.DataBind.SetBound(true, "", "cbbranch");

                cbPosition = oForm.Items.Item("cbposi").Specific;
                oForm.DataSources.UserDataSources.Add("cbposi", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                cbPosition.DataBind.SetBound(true, "", "cbposi");

                cbDepartment = oForm.Items.Item("cbdept").Specific;
                oForm.DataSources.UserDataSources.Add("cbdept", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                cbDepartment.DataBind.SetBound(true, "", "cbdept");

                cbLocation = oForm.Items.Item("cbloc").Specific;
                oForm.DataSources.UserDataSources.Add("cbloc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 20);
                cbLocation.DataBind.SetBound(true, "", "cbloc");

                txtValidityFrom = oForm.Items.Item("txvfrom").Specific;
                oForm.DataSources.UserDataSources.Add("txvfrom", SAPbouiCOM.BoDataType.dt_DATE, 30);
                txtValidityFrom.DataBind.SetBound(true, "", "txvfrom");

                txtValidityTo = oForm.Items.Item("txvto").Specific;
                oForm.DataSources.UserDataSources.Add("txvto", SAPbouiCOM.BoDataType.dt_DATE, 30);
                txtValidityTo.DataBind.SetBound(true, "", "txvto");

                txtVacancyFrom = oForm.Items.Item("txvacf").Specific;
                oForm.DataSources.UserDataSources.Add("txvacf", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER, 10);
                txtVacancyFrom.DataBind.SetBound(true, "", "txvacf");

                txtVacancyTo = oForm.Items.Item("txvact").Specific;
                oForm.DataSources.UserDataSources.Add("txvact", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER, 10);
                txtVacancyTo.DataBind.SetBound(true, "", "txvact");

                txtCandidateNoFrom = oForm.Items.Item("txcannof").Specific;
                oForm.DataSources.UserDataSources.Add("txcannof", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtCandidateNoFrom.DataBind.SetBound(true, "", "txcannof");

                txtCandidateNoTo = oForm.Items.Item("txcannot").Specific;
                oForm.DataSources.UserDataSources.Add("txcannot", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                txtCandidateNoTo.DataBind.SetBound(true, "", "txcannot");

                //Open Tab

                mtOpen = oForm.Items.Item("mtopen").Specific;
                dtOpen = oForm.DataSources.DataTables.Item("dtopen");
                oSerial = mtOpen.Columns.Item("serial");
                oSelected = mtOpen.Columns.Item("selected");
                oFirstName = mtOpen.Columns.Item("fname");
                oLastName = mtOpen.Columns.Item("lname");
                oCandidateNo = mtOpen.Columns.Item("canno");
                oDept = mtOpen.Columns.Item("dept");
                oBranch = mtOpen.Columns.Item("branch");
                oPosition = mtOpen.Columns.Item("position");
                oVfrom = mtOpen.Columns.Item("vfrom");
                oVtill = mtOpen.Columns.Item("vtill");
                oCity = mtOpen.Columns.Item("city");
                oCountry = mtOpen.Columns.Item("country");

                //ShortList Tab
                
                mtShortListed = oForm.Items.Item("mtsl").Specific;
                dtShortListed = oForm.DataSources.DataTables.Item("dtsl");
                sSerial = mtShortListed.Columns.Item("serial");
                sSelected = mtShortListed.Columns.Item("selected");
                sFirstName = mtShortListed.Columns.Item("fname");
                sLastName = mtShortListed.Columns.Item("lname");
                sCandidateNo = mtShortListed.Columns.Item("canno");
                sDept = mtShortListed.Columns.Item("dept");
                sBranch = mtShortListed.Columns.Item("branch");
                sPosition = mtShortListed.Columns.Item("position");
                sVfrom = mtShortListed.Columns.Item("vfrom");
                sVtill = mtShortListed.Columns.Item("vtill");
                sCity = mtShortListed.Columns.Item("city");
                sCountry = mtShortListed.Columns.Item("country");

                btnMain = oForm.Items.Item("btmain").Specific;
                btnSearch = oForm.Items.Item("btsearch").Specific;
                btnReleaseToShortList = oForm.Items.Item("btrsl").Specific;
                btnCreate = oForm.Items.Item("btcombo").Specific;
                btnCreate.ValidValues.Add("Interview", "Create Interview Call");
                btnCreate.ValidValues.Add("Employee", "Create Employee");
                btnCreate.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                
                // Combo Fill Time
                FillStatusCandidate(cbStatus);
                FillDepartment(cbDepartment);
                FillBranch(cbBranch);
                FillLocation(cbLocation);
                FillDesignation(cbPosition);

                dtOpen.Rows.Clear();
                dtShortListed.Rows.Clear();
                oForm.PaneLevel = 1;
                btnMain.Caption = "Ok";
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("InitializeFrom Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void InitiallizeDocument()
        {
            try
            {
                txtBranchFrom.Value = "";
                txtBranchTo.Value = "";
                txtDeptFrom.Value = "";
                txtDeptTo.Value = "";
                txtPositionFrom.Value = "";
                txtPositionTo.Value = "";
                txtVacancyFrom.Value = "";
                txtVacancyTo.Value = "";
                txtValidityFrom.Value = "";
                txtValidityTo.Value = "";

            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}", ex.Message);
            }
        }

        private void CheckMainButtonState()
        {
            switch (btnMain.Caption)
            {
                case "Ok":
                    oForm.Close();
                    break;
                case "Find":
                    SearchCandidate();
                    break;
            }
        }

        private void FillStatusCandidate(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                IEnumerable<MstLOVE> MartialStatus = from a in dbHrPayroll.MstLOVE where a.Type.Contains("Staffing") select a;
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

        private void AddEmptyRowOpen()
        {
            try
            {
                Int32 RowValue = 0;
                if (dtOpen.Rows.Count == 0)
                {
                    dtOpen.Rows.Add(1);
                    RowValue = dtOpen.Rows.Count;
                    dtOpen.SetValue(oSelected.DataBind.Alias, RowValue - 1, "N");
                    dtOpen.SetValue(oFirstName.DataBind.Alias, RowValue - 1, "");
                    dtOpen.SetValue(oLastName.DataBind.Alias, RowValue - 1, "");
                    dtOpen.SetValue(oCandidateNo.DataBind.Alias, RowValue - 1, 0);
                    dtOpen.SetValue(oDept.DataBind.Alias, RowValue - 1, "");
                    dtOpen.SetValue(oBranch.DataBind.Alias, RowValue - 1, "");
                    dtOpen.SetValue(oPosition.DataBind.Alias, RowValue - 1, "");
                    dtOpen.SetValue(oVfrom.DataBind.Alias, RowValue - 1, DateTime.Now);
                    dtOpen.SetValue(oVtill.DataBind.Alias, RowValue - 1, DateTime.Now);
                    dtOpen.SetValue(oCity.DataBind.Alias, RowValue - 1, "");
                    dtOpen.SetValue(oCountry.DataBind.Alias, RowValue - 1, "");
                    mtOpen.AddRow(1, RowValue + 1);
                }
                else
                {
                    if (dtOpen.GetValue(oFirstName.DataBind.Alias, dtOpen.Rows.Count - 1) == "")
                    {
                    }
                    else
                    {
                        mtOpen.FlushToDataSource();
                        dtOpen.Rows.Add(1);
                        RowValue = dtOpen.Rows.Count;
                        dtOpen.SetValue(oSelected.DataBind.Alias, RowValue - 1, "N");
                        dtOpen.SetValue(oFirstName.DataBind.Alias, RowValue - 1, "");
                        dtOpen.SetValue(oLastName.DataBind.Alias, RowValue - 1, "");
                        dtOpen.SetValue(oCandidateNo.DataBind.Alias, RowValue - 1, 0);
                        dtOpen.SetValue(oDept.DataBind.Alias, RowValue - 1, "");
                        dtOpen.SetValue(oBranch.DataBind.Alias, RowValue - 1, "");
                        dtOpen.SetValue(oPosition.DataBind.Alias, RowValue - 1, "");
                        dtOpen.SetValue(oVfrom.DataBind.Alias, RowValue - 1, DateTime.Now);
                        dtOpen.SetValue(oVtill.DataBind.Alias, RowValue - 1, DateTime.Now);
                        dtOpen.SetValue(oCity.DataBind.Alias, RowValue - 1, "");
                        dtOpen.SetValue(oCountry.DataBind.Alias, RowValue - 1, "");
                        mtOpen.AddRow(1, RowValue + 1);
                    }
                }
                mtOpen.LoadFromDataSource();
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }

        }

        private void AddEmptyRowShortList()
        {
            try
            {
                Int32 RowValue = 0;
                if (dtShortListed.Rows.Count == 0)
                {
                    dtShortListed.Rows.Add(1);
                    RowValue = dtShortListed.Rows.Count;
                    dtShortListed.SetValue(oSelected.DataBind.Alias, RowValue - 1, "N");
                    dtShortListed.SetValue(oFirstName.DataBind.Alias, RowValue - 1, "");
                    dtShortListed.SetValue(oLastName.DataBind.Alias, RowValue - 1, "");
                    dtShortListed.SetValue(oCandidateNo.DataBind.Alias, RowValue - 1, "");
                    dtShortListed.SetValue(oDept.DataBind.Alias, RowValue - 1, "");
                    dtShortListed.SetValue(oBranch.DataBind.Alias, RowValue - 1, "");
                    dtShortListed.SetValue(oPosition.DataBind.Alias, RowValue - 1, "");
                    dtShortListed.SetValue(oVfrom.DataBind.Alias, RowValue - 1, DateTime.Now);
                    dtShortListed.SetValue(oVtill.DataBind.Alias, RowValue - 1, DateTime.Now);
                    dtShortListed.SetValue(oCity.DataBind.Alias, RowValue - 1, "");
                    dtShortListed.SetValue(oCountry.DataBind.Alias, RowValue - 1, "");
                    mtShortListed.AddRow(1, RowValue + 1);
                }
                else
                {
                    if (dtShortListed.GetValue(oCandidateNo.DataBind.Alias, dtShortListed.Rows.Count - 1) == "")
                    {
                    }
                    else
                    {
                        mtShortListed.FlushToDataSource();
                        dtShortListed.Rows.Add(1);
                        RowValue = dtShortListed.Rows.Count;
                        dtShortListed.SetValue(oSelected.DataBind.Alias, RowValue - 1, "N");
                        dtShortListed.SetValue(oFirstName.DataBind.Alias, RowValue - 1, "");
                        dtShortListed.SetValue(oLastName.DataBind.Alias, RowValue - 1, "");
                        dtShortListed.SetValue(oCandidateNo.DataBind.Alias, RowValue - 1, "");
                        dtShortListed.SetValue(oDept.DataBind.Alias, RowValue - 1, "");
                        dtShortListed.SetValue(oBranch.DataBind.Alias, RowValue - 1, "");
                        dtShortListed.SetValue(oPosition.DataBind.Alias, RowValue - 1, "");
                        dtShortListed.SetValue(oVfrom.DataBind.Alias, RowValue - 1, DateTime.Now);
                        dtShortListed.SetValue(oVtill.DataBind.Alias, RowValue - 1, DateTime.Now);
                        dtShortListed.SetValue(oCity.DataBind.Alias, RowValue - 1, "");
                        dtShortListed.SetValue(oCountry.DataBind.Alias, RowValue - 1, "");
                        mtShortListed.AddRow(1, RowValue + 1);
                    }
                }
                mtShortListed.LoadFromDataSource();
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void SearchCandidate()
        {
            try
            {
                //DataTable dtCandidates = new DataTable();
                //MstCandidate CandList = null;
                //string statusreport = "";
                //statusreport = cbStatus.Value.Trim();
                //int i = 0;

                //PrepareSearchKeyHash();
                //string query = sqlString.getSql("SLSearch", SearchKeyVal);
                //dtCandidates = ds.getDataTable(query);

                //if (dtCandidates.Rows.Count > 0)
                //{
                //    dtOpen.Rows.Clear();
                //    dtShortListed.Rows.Clear();
                //    foreach (DataRow dr in dtCandidates.Rows)
                //    {
                //        CandList = (from a in dbHrPayroll.MstCandidate where a.ID == Convert.ToInt32(dr[0].ToString()) select a).FirstOrDefault();
                //        String FirstName, LastName, Dept, Branch, Desig, WorkCity, WorkCountry;
                //        Int32 CandidateNo = 0;
                //        DateTime dtFrom, dtTo;
                //        if (String.IsNullOrEmpty(CandList.FirstName))
                //        {
                //            FirstName = "";
                //        }
                //        else
                //        {
                //            FirstName = CandList.FirstName;
                //        }
                //        if (String.IsNullOrEmpty(CandList.LastName))
                //        {
                //            LastName = "";
                //        }
                //        else
                //        {
                //            LastName = CandList.LastName;
                //        }
                //        if (String.IsNullOrEmpty(CandList.WCity))
                //        {
                //            WorkCity = "";
                //        }
                //        else
                //        {
                //            WorkCity = CandList.WCity;
                //        }
                //        if (String.IsNullOrEmpty(CandList.WCountry))
                //        {
                //            WorkCountry = "";
                //        }
                //        else
                //        {
                //            WorkCountry = CandList.WCountry;
                //        }
                //        if (CandList.MstDepartment != null)
                //        {
                //            if (String.IsNullOrEmpty(CandList.MstDepartment.DeptName))
                //            {
                //                Dept = "";
                //            }
                //            else
                //            {
                //                Dept = CandList.MstDepartment.DeptName;
                //            }
                //        }
                //        else
                //        {
                //            Dept = "";
                //        }
                //        if (CandList.MstBranches != null)
                //        {
                //            if (String.IsNullOrEmpty(CandList.MstBranches.Name))
                //            {
                //                Branch = "";
                //            }
                //            else
                //            {
                //                Branch = CandList.MstBranches.Name;
                //            }
                //        }
                //        else
                //        {
                //            Branch = "";
                //        }
                //        if (CandList.MstDesignation != null)
                //        {
                //            if (String.IsNullOrEmpty(CandList.MstDesignation.Name))
                //            {
                //                Desig = "";
                //            }
                //            else
                //            {
                //                Desig = CandList.MstDesignation.Name;
                //            }
                //        }
                //        else
                //        {
                //            Desig = "";
                //        }
                //        if (CandList.CandidateNo != null)
                //        {
                //            CandidateNo = Convert.ToInt32(CandList.CandidateNo);
                //        }
                //        else
                //        {
                //            CandidateNo = 0;
                //        }
                //        if (CandList.ValidFrom != null)
                //        {
                //            dtFrom = Convert.ToDateTime(CandList.ValidFrom);
                //        }
                //        else
                //        {
                //            dtFrom = DateTime.Now.Date;
                //        }
                //        if (CandList.ValidTo != null)
                //        {
                //            dtTo = Convert.ToDateTime(CandList.ValidTo);
                //        }
                //        else
                //        {
                //            dtTo = DateTime.Now.Date;
                //        }
                //        if ( statusreport == "OPEN" && CandList != null)
                //        {
                //            dtOpen.Rows.Add(1);
                //            dtOpen.SetValue(oSelected.DataBind.Alias, i, "N");
                //            dtOpen.SetValue(oFirstName.DataBind.Alias, i, FirstName);
                //            dtOpen.SetValue(oLastName.DataBind.Alias, i, LastName);
                //            dtOpen.SetValue(oCandidateNo.DataBind.Alias, i, CandidateNo);
                //            dtOpen.SetValue(oDept.DataBind.Alias, i, Dept);
                //            dtOpen.SetValue(oBranch.DataBind.Alias, i, Branch);
                //            dtOpen.SetValue(oPosition.DataBind.Alias, i, Desig);
                //            dtOpen.SetValue(oVfrom.DataBind.Alias, i, dtFrom);
                //            dtOpen.SetValue(oVtill.DataBind.Alias, i, dtTo);
                //            dtOpen.SetValue(oCity.DataBind.Alias, i, WorkCity);
                //            dtOpen.SetValue(oCountry.DataBind.Alias, i, WorkCountry);
                //        }
                //        else if (statusreport == "SHTLST" && CandList != null)
                //        {
                //            dtShortListed.Rows.Add(1);
                //            dtShortListed.SetValue(sSelected.DataBind.Alias, i, "N");
                //            dtShortListed.SetValue(sFirstName.DataBind.Alias, i, FirstName);
                //            dtShortListed.SetValue(sLastName.DataBind.Alias, i, LastName);
                //            dtShortListed.SetValue(sCandidateNo.DataBind.Alias, i, CandidateNo);
                //            dtShortListed.SetValue(sDept.DataBind.Alias, i, Dept);
                //            dtShortListed.SetValue(sPosition.DataBind.Alias, i, Desig);
                //            dtShortListed.SetValue(sBranch.DataBind.Alias, i, Branch);
                //            dtShortListed.SetValue(sVfrom.DataBind.Alias, i, dtFrom);
                //            dtShortListed.SetValue(sVtill.DataBind.Alias, i, dtTo);
                //            dtShortListed.SetValue(sCity.DataBind.Alias, i, WorkCity);
                //            dtShortListed.SetValue(sCountry.DataBind.Alias, i, WorkCountry);
                //        }
                //        i++;
                //    }// end of foreach loop
                //    mtOpen.LoadFromDataSource();
                //    mtShortListed.LoadFromDataSource();
                //}// end of candidate null
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void ReleaseToShortList()
        {
            try
            {
                //List<Int32> Candidate = new List<Int32>();
                //mtOpen.FlushToDataSource();
                //for (Int32 i = 0; i < dtOpen.Rows.Count; i++)
                //{
                //    Int32 CandidateNo = dtOpen.GetValue(oCandidateNo.DataBind.Alias, i);
                //    if (CandidateNo != 0)
                //    {
                //        String IsSelected = dtOpen.GetValue(oSelected.DataBind.Alias,i);
                //        if (IsSelected == "Y")
                //        {
                //            Candidate.Add(CandidateNo);
                //        }
                //    }
                //}
                //mtOpen.LoadFromDataSource();
                //dtShortListed.Rows.Clear();
                //dtShortListed.Rows.Add(Candidate.Count);
                //for (Int32 i = 0; i < Candidate.Count; i++)
                //{
                //    MstCandidate OneCandidate = (from a in dbHrPayroll.MstCandidate where Convert.ToString(a.CandidateNo) == Candidate.ElementAt(i).ToString() select a).FirstOrDefault();
                //    String FirstName, LastName, Dept, Branch, Desig, WorkCity, WorkCountry;
                //    Int32 CandidateNo = 0;
                //    DateTime dtFrom, dtTo;
                //    if (String.IsNullOrEmpty(OneCandidate.FirstName))
                //    {
                //        FirstName = "";
                //    }
                //    else
                //    {
                //        FirstName = OneCandidate.FirstName;
                //    }
                //    if (String.IsNullOrEmpty(OneCandidate.LastName))
                //    {
                //        LastName = "";
                //    }
                //    else
                //    {
                //        LastName = OneCandidate.LastName;
                //    }
                //    if (String.IsNullOrEmpty(OneCandidate.WCity))
                //    {
                //        WorkCity = "";
                //    }
                //    else
                //    {
                //        WorkCity = OneCandidate.WCity;
                //    }
                //    if (String.IsNullOrEmpty(OneCandidate.WCountry))
                //    {
                //        WorkCountry = "";
                //    }
                //    else
                //    {
                //        WorkCountry = OneCandidate.WCountry;
                //    }
                //    if (OneCandidate.MstDepartment != null)
                //    {
                //        if (String.IsNullOrEmpty(OneCandidate.MstDepartment.DeptName))
                //        {
                //            Dept = "";
                //        }
                //        else
                //        {
                //            Dept = OneCandidate.MstDepartment.DeptName;
                //        }
                //    }
                //    else
                //    {
                //        Dept = "";
                //    }
                //    if (OneCandidate.MstBranches != null)
                //    {
                //        if (String.IsNullOrEmpty(OneCandidate.MstBranches.Name))
                //        {
                //            Branch = "";
                //        }
                //        else
                //        {
                //            Branch = OneCandidate.MstBranches.Name;
                //        }
                //    }
                //    else
                //    {
                //        Branch = "";
                //    }
                //    if (OneCandidate.MstDesignation != null)
                //    {
                //        if (String.IsNullOrEmpty(OneCandidate.MstDesignation.Name))
                //        {
                //            Desig = "";
                //        }
                //        else
                //        {
                //            Desig = OneCandidate.MstDesignation.Name;
                //        }
                //    }
                //    else
                //    {
                //        Desig = "";
                //    }
                //    if (OneCandidate.CandidateNo != null)
                //    {
                //        CandidateNo = Convert.ToInt32(OneCandidate.CandidateNo);
                //    }
                //    else
                //    {
                //        CandidateNo = 0;
                //    }
                //    if (OneCandidate.ValidFrom != null)
                //    {
                //        dtFrom = Convert.ToDateTime(OneCandidate.ValidFrom);
                //    }
                //    else
                //    {
                //        dtFrom = DateTime.Now.Date;
                //    }
                //    if (OneCandidate.ValidTo != null)
                //    {
                //        dtTo = Convert.ToDateTime(OneCandidate.ValidTo);
                //    }
                //    else
                //    {
                //        dtTo = DateTime.Now.Date;
                //    }
                //    OneCandidate.StaffingStatus = "SHTLST";
                //    dtShortListed.SetValue(sSelected.DataBind.Alias, i, "N");
                //    dtShortListed.SetValue(sFirstName.DataBind.Alias, i, FirstName);
                //    dtShortListed.SetValue(sLastName.DataBind.Alias, i, LastName);
                //    dtShortListed.SetValue(sCandidateNo.DataBind.Alias, i, CandidateNo);
                //    dtShortListed.SetValue(sDept.DataBind.Alias, i, Dept);
                //    dtShortListed.SetValue(sPosition.DataBind.Alias, i, Desig);
                //    dtShortListed.SetValue(sBranch.DataBind.Alias, i, Branch);
                //    dtShortListed.SetValue(sVfrom.DataBind.Alias, i, dtFrom);
                //    dtShortListed.SetValue(sVtill.DataBind.Alias, i, dtTo);
                //    dtShortListed.SetValue(sCity.DataBind.Alias, i, WorkCity);
                //    dtShortListed.SetValue(sCountry.DataBind.Alias, i, WorkCountry);
                //    dbHrPayroll.SubmitChanges();
                //}
                //mtShortListed.LoadFromDataSource();
                ////SearchCandidate();
                //oApplication.StatusBar.SetText("Candidate Shortlisted Successfully", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Candidate Shortlisted Un-Successfully Error : "+ Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void CreateInterviewCall()
        {
            try
            {
                //List<Int32> Candidate = new List<Int32>();
                //mtShortListed.FlushToDataSource();
                //for (Int32 i = 0; i < dtShortListed.Rows.Count; i++)
                //{
                //    Int32 CandidateNo = dtShortListed.GetValue(sCandidateNo.DataBind.Alias, i);
                //    if (CandidateNo != 0)
                //    {
                //        String IsSelected = dtShortListed.GetValue(sSelected.DataBind.Alias, i);
                //        if (IsSelected == "Y")
                //        {
                //            Candidate.Add(CandidateNo);
                //        }
                //    }
                //}
                //if (Candidate.Count < 1)
                //{
                //    oApplication.SetStatusBarMessage(Program.objHrmsUI.getStrMsg("INF_SelectCandidate"), SAPbouiCOM.BoMessageTime.bmt_Medium, true);
                //    return;
                //}
                //for (Int32 i = 0; i < Candidate.Count; i++)
                //{
                //    MstCandidate oCan = (from a in dbHrPayroll.MstCandidate where Convert.ToString(a.CandidateNo) == Candidate.ElementAtOrDefault(i).ToString() select a).FirstOrDefault();
                //    TrnsInterviewCall oNew = new TrnsInterviewCall();
                //    oNew.CandidateID = oCan.ID;
                //    oNew.DocNum = ds.GetDocumentNumber(-1, 22);
                //    oNew.CreateDt = DateTime.Now;
                //    oNew.UserId = oCompany.UserName;
                //    oNew.DocStatus = "LV0001";
                //    oNew.Series = -1;
                //    oNew.DocType = 15;
                //    dbHrPayroll.TrnsInterviewCall.InsertOnSubmit(oNew);
                //    oCan.StaffingStatus = "InProcess";
                //    dbHrPayroll.SubmitChanges();
                //    oApplication.StatusBar.SetText("Interview Call Created Successfully", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                //    btnMain.Caption = "Ok";
                //}
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void CreateEmployee()
        {
            //try
            //{
                
            //    List<Int32> Candidate = new List<Int32>();
            //    mtShortListed.FlushToDataSource();
            //    for (Int32 i = 0; i < dtShortListed.Rows.Count; i++)
            //    {
            //        Int32 CandidateNo = dtShortListed.GetValue(sCandidateNo.DataBind.Alias, i);
            //        if (CandidateNo != 0)
            //        {
            //            String IsSelected = dtShortListed.GetValue(sSelected.DataBind.Alias, i);
            //            if (IsSelected == "Y")
            //            {
            //                Candidate.Add(CandidateNo);
            //            }
            //        }
            //    }
                
            //    if (Candidate.Count < 1)
            //    {
            //        oApplication.StatusBar.SetText("Select candidate first", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
            //        return;
            //    }
            //    for (Int32 i = 0; i < Candidate.Count; i++)
            //    {
            //        Int32 candidatenumber = 0;

            //        candidatenumber = Convert.ToInt32(Candidate.ElementAtOrDefault(i).ToString());
            //        MstCandidate oCan = (from a in dbHrPayroll.MstCandidate where a.CandidateNo == candidatenumber && a.StaffingStatus.Contains("SHTLST") select a).FirstOrDefault();
            //        if (oCan == null)
            //        {
            //            oApplication.StatusBar.SetText("Candidate is not shortlisted Candidate No " + candidatenumber.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            //            continue;
            //        }

            //        MstEmployee oEmp = new MstEmployee();
            //        MstUsers oUser = new MstUsers();
            //        if (String.IsNullOrEmpty(oCan.UserCode))
            //        {
            //            oApplication.StatusBar.SetText("Update UserCode in Candidate Master, Employee not generated", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            //            return;
            //        }
            //        if (String.IsNullOrEmpty(oCan.EmpCode))
            //        {
            //            oApplication.StatusBar.SetText("Update EMPCode in Candidate Master, Employee not generated", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            //            return;
            //        }
            //        oUser.UserCode = oCan.UserCode;
            //        oUser.UserID = oCan.UserCode;
            //        oUser.PassCode = "12345";
            //        oEmp.EmpID = oCan.EmpCode;
            //        oEmp.FirstName = oCan.FirstName;
            //        oEmp.MiddleName = oCan.MiddleName != null ? oCan.MiddleName : null;
            //        oEmp.LastName = oCan.LastName;
            //        if (oCan.Position != null)
            //        {
            //            oEmp.PositionID = oCan.MstPosition.Id;
            //        }
            //        else
            //        {
            //            oEmp.PositionID = null;
            //        }
            //        if (oCan.Branch != null)
            //        {
            //            oEmp.BranchID = oCan.MstBranches.Id;
            //        }
            //        else
            //        {
            //            oEmp.BranchID = null;
            //        }
            //        if (oCan.Department != null)
            //        {
            //            oEmp.DepartmentID = oCan.MstDepartment.ID;
            //        }
            //        else
            //        {
            //            oEmp.DepartmentID = null;
            //        }
            //        if (oCan.Location != null)
            //        {
            //            oEmp.Location = oCan.MstLocation.Id;
            //        }
            //        else
            //        {
            //            oEmp.Location = null;
            //        }
            //        oEmp.OfficePhone = oCan.OfficePhone;
            //        oEmp.HomePhone = oCan.HomePhone;
            //        oEmp.OfficeMobile = oCan.MobilePhone;
            //        oEmp.OfficeExtension = oCan.Extension;
            //        oEmp.Pager = oCan.Pager;
            //        oEmp.Fax = oCan.Fax;
            //        //oEmp.OfficeEmail = oCan.Email;
            //        oEmp.BasicSalary = 0.0M;
            //        oEmp.FlgActive = true;
            //        oEmp.FlgUser = true;
            //        oEmp.IntSboPublished = false;
            //        oEmp.IntSboTransfered = false;
            //        oEmp.BasicSalary = 0.0M;

            //        oEmp.CreateDate = DateTime.Now;
            //        oEmp.UserId = oCompany.UserName;
            //        oEmp.UpdateDate = DateTime.Now;
            //        oEmp.UpdatedBy = oCompany.UserName;
            //        oUser.CreateDate = DateTime.Now;
            //        oUser.UpdateDate = DateTime.Now;
            //        oUser.CreatedBy = oCompany.UserName;
            //        oUser.UpdatedBy = oCompany.UserName;
                    
            //        oEmp.MstUsers.Add(oUser);
            //        oCan.StaffingStatus = "Selected";
            //        dbHrPayroll.MstEmployee.InsertOnSubmit(oEmp);
            //        dbHrPayroll.SubmitChanges();
            //        oApplication.StatusBar.SetText("Employee Added Successfully", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            //    }
            //}
            //catch (Exception Ex)
            //{
            //    oApplication.StatusBar.SetText("Employee Creation Unsuccessfull Error : " + Ex.Message , SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            //}
        }

        private void FillLocation(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                IEnumerable<MstLocation> oLoc = from a in dbHrPayroll.MstLocation select a;
                pCombo.ValidValues.Add("-1", "");
                foreach (MstLocation OneLocation in oLoc)
                {
                    pCombo.ValidValues.Add(Convert.ToString(OneLocation.Id), Convert.ToString(OneLocation.Name));
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void FillBranch(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                IEnumerable<MstBranches> oCollection = from a in dbHrPayroll.MstBranches select a;
                pCombo.ValidValues.Add("-1", "");
                foreach (MstBranches One in oCollection)
                {
                    pCombo.ValidValues.Add(Convert.ToString(One.Id), Convert.ToString(One.Name));
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void FillDepartment(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                IEnumerable<MstDepartment> oCollection = from a in dbHrPayroll.MstDepartment select a;
                pCombo.ValidValues.Add("-1", "");
                foreach (MstDepartment One in oCollection)
                {
                    pCombo.ValidValues.Add(Convert.ToString(One.ID), Convert.ToString(One.DeptName));
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void FillDesignation(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                IEnumerable<MstDesignation> oCollection = from a in dbHrPayroll.MstDesignation select a;
                pCombo.ValidValues.Add("-1", "");
                foreach (MstDesignation One in oCollection)
                {
                    pCombo.ValidValues.Add(Convert.ToString(One.Id), Convert.ToString(One.Name));
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        #endregion
    }
}
