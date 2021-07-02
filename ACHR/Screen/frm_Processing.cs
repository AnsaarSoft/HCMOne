using System;
using System.Data;
using System.Collections;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using DIHRMS.Custom;
using SAPbobsCOM;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.ComponentModel;
using System.IO;

namespace ACHR.Screen
{
    class frm_Processing : HRMSBaseForm
    {

        #region Variable

        SAPbouiCOM.Matrix grdEmpProc, grdEmpPost, grdElem, grdOthElem, grdJeDet, grdJe;
        SAPbouiCOM.EditText txEmpFrom, txEmpTo, txBasic, txEleTot, txOtTot, txNet, txTotDeb, txTotCred, txtSrchNPEmployyes, txtSrchPEmployyes, txtPostingDate;
        SAPbouiCOM.ComboBox cbPayroll, cbPeriod, cbDept, cbLoc, cmb_ColNPEmployyes, cmb_ColPEmployyes, cmbBranch;
        SAPbouiCOM.Button btProcess, btVoid, btnOK, btPrintSlp, btnExportSIF, btPrintSh, btPostSbo, btPost, btCancelJe, btPayment, btDepartSheet, btnSearchNPEmployyes, btnSearchPEmployyes, btnResendEmail, btnSendEmail;
        SAPbouiCOM.OptionBtn optNPost;
        SAPbouiCOM.OptionBtn optPost, optSendEmail;
        SAPbouiCOM.Item ItxNet, ItxBasic, ItxEleTot, ItxOtTot, ItxEmpFrom, ItxEmpTo, IcbPayroll, IcbPeriod, IcbYear, IcbDept, IcbLoc, IbtProcess, IbtnOK, IbtPostSbo, IbtVoid, IbtPrintSlp, IbtPrintSh, IbtPost, ItxTotDeb, ItxTotCred, IbtCancelJe, IbtPayment, ibtnExportSIF, IbtnResendEmail, IbtnSendEmail, IcmbBranch, itxtPostingDate;
        SAPbouiCOM.DataTable dtPeriods, dtEmpsPr, dtEmpsPost, dtPrEle, dtPrOth, dtJE, dtJeDet, dtHead;
        public string selJe = "";
        public DateTime PeriodStartDate, PeriodEndDate;
        private bool periodLocked = false;
        private int RoundingSet = 0;
        int currentRowIndex = 1;
        string SearchText = "", CompanyName = "";
        private bool isFormLoad = false;

        #endregion

        #region B1 Events

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            InitiallizeForm();
            IbtnOK.Visible = false;
            IbtnResendEmail.Visible = false;
            FillColumnsNPEmployees();
            FillColumnsPEmployees();
        }

        public override void etAfterCmbSelect(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {

            try
            {
                base.etAfterCmbSelect(ref pVal, ref BubbleEvent);
                switch (pVal.ItemUID)
                {
                    case "cbPayroll":
                    case "cbDept":
                    case "cbLoc":
                    case "cbPeriod":
                        refreshEmps();
                        break;
                }
                if (pVal.ItemUID == "cbPayroll")
                {
                    if (isFormLoad)
                    {
                        FillPeriod(cbPayroll.Value);
                    }
                }

            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
            }

        }

        private void refreshEmps()
        {
            try
            {
                if (cbPeriod.Value.ToString() != "")
                {
                    CfgPeriodDates periods = (from p in dbHrPayroll.CfgPeriodDates where p.ID.ToString() == cbPeriod.Value.ToString() select p).Single();
                    PeriodStartDate = Convert.ToDateTime(periods.StartDate);
                    PeriodEndDate = Convert.ToDateTime(periods.EndDate);

                    getNPEmployees();
                    getPEmployees();
                    getEmailEmployees();

                    if (cbPeriod.Value != "" && periods.FlgLocked == true)
                    {
                        IbtProcess.Enabled = false;
                        IbtVoid.Enabled = false;
                        IbtPost.Enabled = false;
                        //Enhanced Code
                        IbtPost.Enabled = false;
                        IbtPostSbo.Enabled = false;

                    }
                    else
                    {
                        IbtProcess.Enabled = true;
                        IbtVoid.Enabled = true;
                        IbtPost.Enabled = true;
                        //Enhanced Code
                        IbtPost.Enabled = true;
                        if (Program.objHrmsUI.isSuperUser)
                        {
                            IbtPostSbo.Enabled = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
            }
        }

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            try
            {
                base.etAfterClick(ref pVal, ref BubbleEvent);
                if (pVal.ItemUID == "btProcess")
                {
                    ProcessingChecks();
                }
                if (pVal.ItemUID == "optNPost")
                {
                    getPEmployees();
                }
                if (pVal.ItemUID == "optPost")
                {
                    getPEmployees();
                }
                if (pVal.ItemUID == "optEmail")
                {
                    getEmailEmployees();
                }
                if (pVal.ItemUID == "btPost")
                {
                    if (Convert.ToBoolean(Program.systemInfo.FlgCostCenterGL))
                    {
                        PostSalaryCostCenter();
                    }
                    else if (Convert.ToBoolean(Program.systemInfo.FlgMultipleDimension))
                    {
                        postSalaryDimensionWise();
                    }
                    else if (Convert.ToBoolean(Program.systemInfo.FlgBranches))
                    {
                        postBranches();
                    }
                    else
                    {
                        int confirm = oApplication.MessageBox("JE posting is irr-reversable. Are you sure you want to post salary? ", 3, "Yes", "No", "Cancel");
                        if (confirm == 2 || confirm == 3) return;
                        if (Convert.ToBoolean(Program.systemInfo.FlgA1Integration))
                        {
                            PostA1IntegrationJE();
                            PostSalary();
                        }

                        else
                        {
                            if (Convert.ToBoolean(Program.systemInfo.FlgJELocationWise))
                            {
                                ClassifyAlreadyProcessedSalary();
                                PostSalaryClassified();
                            }
                            else
                            {
                                PostSalary();
                            }
                        }
                    }
                }

                if (pVal.ItemUID == "btCancelJe")
                {
                    cancelDraft();
                }

                if (pVal.ItemUID == "btPostSbo")
                {
                    if (postIntoSbo())
                    {
                        IbtPostSbo.Enabled = false;
                        IbtCancelJe.Enabled = false;
                    }
                }

                if (pVal.ItemUID == "btVoid")
                {
                    VoidSalary();
                }
                if (pVal.ItemUID == "btPrintSlp")
                {
                    printSlip();
                }
                if (pVal.ItemUID == "btREmail")
                {
                    //printSlip();
                    reSendEmailSlip();
                }
                if (pVal.ItemUID == "btEmail")
                {
                    //printSlip();
                    emailSlip();
                }

                if (pVal.ItemUID == "btSif")
                {
                    ExportTosif();
                }
                if (pVal.ItemUID == "btPayment")
                {
                    printPmt();
                }
                if (pVal.ItemUID == "btPrintSh")
                {
                    printSheet();
                }
                if (pVal.ItemUID == "bttax")
                {
                    TaxDetailInfo();
                }
                if (pVal.ItemUID == "btdpt")
                {
                    printDepartSheet();
                }

                if (pVal.ItemUID == "mtJE")
                {

                    if (pVal.Row >= 1 && pVal.Row <= grdJe.RowCount)
                    {
                        try
                        {
                            string jeNum = Convert.ToString(dtJE.GetValue("jeNum", pVal.Row - 1));
                            selJe = jeNum;
                            if (Convert.ToString(dtJE.GetValue("SBOJe", pVal.Row - 1)) == "")
                            {
                                IbtCancelJe.Enabled = true;
                                if (Program.objHrmsUI.isSuperUser)
                                {
                                    IbtPostSbo.Enabled = true;
                                }
                            }
                            else
                            {
                                IbtCancelJe.Enabled = false;
                                IbtPostSbo.Enabled = false;
                                IbtnSendEmail.Enabled = false;
                            }
                            if (Convert.ToBoolean(Program.systemInfo.FlgJELocationWise))
                            {
                                fillJeDetailLocation(jeNum);
                            }
                            else
                            {
                                fillJeDetail(jeNum);
                            }
                            //fillJeDetail(jeNum);
                        }
                        catch
                        {
                            selJe = "";
                            // iniSalaryDetail();
                        }
                    }
                }

                if (pVal.ItemUID == "mtEmpPost")
                {

                    if (pVal.Row >= 1 && pVal.Row <= grdEmpPost.RowCount)
                    {
                        try
                        {
                            string id = Convert.ToString(dtEmpsPost.GetValue("id", pVal.Row - 1));
                            fillSalaryDetails(id);
                        }
                        catch
                        {
                            // iniSalaryDetail();
                        }
                    }
                    if (pVal.ColUID == "isSel" && pVal.Row == 0)
                    {
                        selectAllPost();
                    }
                }
                if (pVal.ItemUID == "mtEmpPr")
                {

                    if (pVal.ColUID == "isSel" && pVal.Row == 0)
                    {
                        selectAllProcess();
                    }
                }
                if (pVal.ItemUID == "56")
                {
                    FilterRecordNPEmployees();
                }
                if (pVal.ItemUID == "60")
                {
                    FilterRecordPEmployees();
                }
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
            }
        }

        public override void etAfterKeyDown(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterKeyDown(ref pVal, ref BubbleEvent);
            if (pVal.CharPressed == 13)
            {
                if (pVal.ItemUID == "mtEmpPr" || pVal.ItemUID == "55" || pVal.ItemUID == "56")
                {
                    SearchText = txtSrchNPEmployyes.Value.Trim();

                    if (pVal.Row >= -1 && currentRowIndex >= 1)
                    {
                        if (currentRowIndex > 0)
                        {
                            if (SearchText == txtSrchNPEmployyes.Value.Trim())
                            {
                                FilterRecordNPEmployees();
                            }
                        }
                    }
                }
                if (pVal.ItemUID == "mtEmpPost" || pVal.ItemUID == "59" || pVal.ItemUID == "60")
                {
                    SearchText = txtSrchPEmployyes.Value.Trim();
                    if (pVal.Row >= -1 && currentRowIndex >= 1)
                    {
                        if (currentRowIndex > 0)
                        {
                            if (SearchText == txtSrchPEmployyes.Value.Trim())
                            {
                                FilterRecordPEmployees();
                            }
                        }
                    }
                }
            }
        }

        private void selectAllPost()
        {
            try
            {

                oForm.Freeze(true);
                SAPbouiCOM.Column col = grdEmpPost.Columns.Item("isSel");

                if (col.TitleObject.Caption == "X")
                {
                    for (int i = 0; i < dtEmpsPost.Rows.Count; i++)
                    {

                        dtEmpsPost.SetValue("isSel", i, "N");
                        col.TitleObject.Caption = "";
                    }
                }
                else
                {
                    for (int i = 0; i < dtEmpsPost.Rows.Count; i++)
                    {
                        dtEmpsPost.SetValue("isSel", i, "Y");
                        col.TitleObject.Caption = "X";
                    }
                }
                grdEmpPost.LoadFromDataSource();
                oForm.Freeze(false);
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
                // iniSalaryDetail();
            }
        }

        private void selectAllProcess()
        {
            try
            {

                oForm.Freeze(true);
                SAPbouiCOM.Column col = grdEmpProc.Columns.Item("isSel");

                if (col.TitleObject.Caption == "X")
                {
                    for (int i = 0; i < dtEmpsPr.Rows.Count; i++)
                    {

                        dtEmpsPr.SetValue("isSel", i, "N");
                        col.TitleObject.Caption = "";
                    }
                }
                else
                {
                    for (int i = 0; i < dtEmpsPr.Rows.Count; i++)
                    {
                        dtEmpsPr.SetValue("isSel", i, "Y");
                        col.TitleObject.Caption = "X";
                    }
                }
                grdEmpProc.LoadFromDataSource();
                oForm.Freeze(false);
            }

            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
                // iniSalaryDetail();
            }
        }

        #endregion

        #region Functions

        private void InitiallizeForm()
        {
            oForm.Freeze(true);
            try
            {
                CompanyName = string.IsNullOrEmpty(Program.systemInfo.CompanyName) ? "" : Program.systemInfo.CompanyName.Trim();
                if (ACHR.Properties.Settings.Default.RoundingValue == "Yes")
                {
                    RoundingSet = 1;
                }
                else
                {
                    RoundingSet = 0;
                }

                dtHead = oForm.DataSources.DataTables.Item("dtHead");
                dtHead.Rows.Add(1);
                optNPost = oForm.Items.Item("optNPost").Specific;
                optPost = oForm.Items.Item("optPost").Specific;
                optSendEmail = oForm.Items.Item("optEmail").Specific;

                optNPost.GroupWith("optPost");
                optSendEmail.GroupWith("optPost");
                optNPost.Selected = true;
                dtHead.SetValue("optNP", 0, "Y");


                oForm.DataSources.UserDataSources.Add("txEmpFrom", SAPbouiCOM.BoDataType.dt_LONG_NUMBER); // Days of Month
                txEmpFrom = oForm.Items.Item("txEmpFrom").Specific;
                ItxEmpFrom = oForm.Items.Item("txEmpFrom");
                txEmpFrom.DataBind.SetBound(true, "", "txEmpFrom");

                oForm.DataSources.UserDataSources.Add("txEmpTo", SAPbouiCOM.BoDataType.dt_LONG_NUMBER); // Days of Month
                txEmpTo = oForm.Items.Item("txEmpTo").Specific;
                ItxEmpTo = oForm.Items.Item("txEmpTo");
                txEmpTo.DataBind.SetBound(true, "", "txEmpTo");

                oForm.DataSources.UserDataSources.Add("txOtTot", SAPbouiCOM.BoDataType.dt_SUM); // Days of Month
                txOtTot = oForm.Items.Item("txOtTot").Specific;
                ItxOtTot = oForm.Items.Item("txOtTot");
                txOtTot.DataBind.SetBound(true, "", "txOtTot");

                oForm.DataSources.UserDataSources.Add("txEleTot", SAPbouiCOM.BoDataType.dt_SUM); // Days of Month
                txEleTot = oForm.Items.Item("txEleTot").Specific;
                ItxEleTot = oForm.Items.Item("txEleTot");
                txEleTot.DataBind.SetBound(true, "", "txEleTot");

                oForm.DataSources.UserDataSources.Add("txBasic", SAPbouiCOM.BoDataType.dt_SUM); // Days of Month
                txBasic = oForm.Items.Item("txBasic").Specific;
                ItxBasic = oForm.Items.Item("txBasic");
                txBasic.DataBind.SetBound(true, "", "txBasic");


                oForm.DataSources.UserDataSources.Add("txNet", SAPbouiCOM.BoDataType.dt_SUM); // Days of Month
                txNet = oForm.Items.Item("txNet").Specific;
                ItxNet = oForm.Items.Item("txNet");
                txNet.DataBind.SetBound(true, "", "txNet");

                oForm.DataSources.UserDataSources.Add("txTotDeb", SAPbouiCOM.BoDataType.dt_SUM); // Days of Month
                txTotDeb = oForm.Items.Item("txTotDeb").Specific;
                ItxTotDeb = oForm.Items.Item("txTotDeb");
                txTotDeb.DataBind.SetBound(true, "", "txTotDeb");

                oForm.DataSources.UserDataSources.Add("txTotCred", SAPbouiCOM.BoDataType.dt_SUM); // Days of Month
                txTotCred = oForm.Items.Item("txTotCred").Specific;
                ItxTotCred = oForm.Items.Item("txTotCred");
                txTotCred.DataBind.SetBound(true, "", "txTotCred");

                oForm.DataSources.UserDataSources.Add("txpdt", SAPbouiCOM.BoDataType.dt_DATE);
                txtPostingDate = oForm.Items.Item("txpdt").Specific;
                itxtPostingDate = oForm.Items.Item("txpdt");
                txtPostingDate.DataBind.SetBound(true, "", "txpdt");

                oForm.DataSources.UserDataSources.Add("cbPayroll", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
                cbPayroll = oForm.Items.Item("cbPayroll").Specific;
                IcbPayroll = oForm.Items.Item("cbPayroll");
                cbPayroll.DataBind.SetBound(true, "", "cbPayroll");

                oForm.DataSources.UserDataSources.Add("cbPeriod", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
                cbPeriod = oForm.Items.Item("cbPeriod").Specific;
                IcbPeriod = oForm.Items.Item("cbPeriod");
                cbPeriod.DataBind.SetBound(true, "", "cbPeriod");

                oForm.DataSources.UserDataSources.Add("cbDept", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
                cbDept = oForm.Items.Item("cbDept").Specific;
                IcbDept = oForm.Items.Item("cbDept");
                cbDept.DataBind.SetBound(true, "", "cbDept");

                oForm.DataSources.UserDataSources.Add("cbLoc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
                cbLoc = oForm.Items.Item("cbLoc").Specific;
                IcbLoc = oForm.Items.Item("cbLoc");
                cbLoc.DataBind.SetBound(true, "", "cbLoc");

                cmbBranch = oForm.Items.Item("cbBrnch").Specific;
                oForm.DataSources.UserDataSources.Add("cbBrnch", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 100);
                cmbBranch.DataBind.SetBound(true, "", "cbBrnch");

                IcmbBranch = oForm.Items.Item("cbBrnch");

                dtPeriods = oForm.DataSources.DataTables.Item("dtPeriods");

                grdEmpProc = oForm.Items.Item("mtEmpPr").Specific;
                dtEmpsPr = oForm.DataSources.DataTables.Item("dtEmpsPr");

                grdEmpPost = oForm.Items.Item("mtEmpPost").Specific;
                dtEmpsPost = oForm.DataSources.DataTables.Item("dtEmpsPost");

                dtPrEle = oForm.DataSources.DataTables.Item("dtPrEle");
                grdElem = oForm.Items.Item("mtEle").Specific;

                dtPrOth = oForm.DataSources.DataTables.Item("dtPrOth");
                grdOthElem = oForm.Items.Item("mtPrOth").Specific;

                dtJE = oForm.DataSources.DataTables.Item("dtJE");
                grdJe = oForm.Items.Item("mtJE").Specific;
                dtJeDet = oForm.DataSources.DataTables.Item("dtJeDet");
                grdJeDet = oForm.Items.Item("mtJeDet").Specific;

                //btnOK = oForm.Items.Item("1").Specific;



                btProcess = oForm.Items.Item("btProcess").Specific;
                IbtProcess = oForm.Items.Item("btProcess");
                IbtVoid = oForm.Items.Item("btVoid");
                btVoid = oForm.Items.Item("btVoid").Specific;
                btnOK = oForm.Items.Item("1").Specific;
                IbtnOK = oForm.Items.Item("1");
                grdEmpPost.Columns.Item("id").Visible = false;

                btPostSbo = oForm.Items.Item("btPostSbo").Specific;
                IbtPostSbo = oForm.Items.Item("btPostSbo");
                btPost = oForm.Items.Item("btPost").Specific;
                IbtPost = oForm.Items.Item("btPost");
                btCancelJe = oForm.Items.Item("btCancelJe").Specific;
                IbtCancelJe = oForm.Items.Item("btCancelJe");
                btnExportSIF = oForm.Items.Item("btSif").Specific;
                ibtnExportSIF = oForm.Items.Item("btSif");
                if (Convert.ToBoolean(Program.systemInfo.FlgArabic))
                {
                    ibtnExportSIF.Visible = true;
                    ibtnExportSIF.Enabled = true;
                }
                else
                {
                    ibtnExportSIF.Visible = false;
                    ibtnExportSIF.Enabled = false;
                }

                fillCbs();
                getNPEmployees();
                getPEmployees();
                oForm.PaneLevel = 1;
                //Search Items
                btnSearchNPEmployyes = oForm.Items.Item("56").Specific;
                txtSrchNPEmployyes = oForm.Items.Item("55").Specific;
                cmb_ColNPEmployyes = oForm.Items.Item("54").Specific;

                btnSearchPEmployyes = oForm.Items.Item("60").Specific;
                txtSrchPEmployyes = oForm.Items.Item("59").Specific;
                cmb_ColPEmployyes = oForm.Items.Item("58").Specific;
                //
                //Email
                btnSendEmail = oForm.Items.Item("btEmail").Specific;
                IbtnSendEmail = oForm.Items.Item("btEmail");

                btnResendEmail = oForm.Items.Item("btREmail").Specific;
                IbtnResendEmail = oForm.Items.Item("btREmail");
                IbtnResendEmail.Visible = false;
                IbtnSendEmail.Enabled = false;
                //
                //mtEmpPost.Columns.Item("id").TitleObject.Sortable = false;
                //mtEmpPost.Columns.Item("id").TitleObject.Sortable = false;
                //mtEmpPr.Columns.Item("id").TitleObject.Sortable = false;

                // btnOK.Caption = "OK";
                grdElem.AutoResizeColumns();
                grdOthElem.AutoResizeColumns();
                grdEmpPost.AutoResizeColumns();
                grdEmpProc.AutoResizeColumns();
                grdJe.AutoResizeColumns();
                grdJeDet.AutoResizeColumns();
                isFormLoad = true;
            }
            catch (Exception ex)
            {
            }
            oForm.Freeze(false);
        }

        private void FillColumnsNPEmployees()
        {
            try
            {

                int i = 0;
                ArrayList list = new ArrayList();
                list.Add("EmpID");
                list.Add("FirstName");
                list.Add("MiddleName");
                list.Add("LastName");
                list.Add("BranchName");

                if (list.Count > 0)
                {
                    cmb_ColNPEmployyes.ValidValues.Add("-1", "[select one]");
                    foreach (var v in list)
                    {
                        cmb_ColNPEmployyes.ValidValues.Add(Convert.ToString(v), Convert.ToString(v));
                    }
                    cmb_ColNPEmployyes.Select(1, SAPbouiCOM.BoSearchKey.psk_Index);
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("FillColumnsNPEmployees Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillColumnsPEmployees()
        {
            try
            {
                int i = 0;
                ArrayList list = new ArrayList();
                list.Add("EmpID");
                list.Add("FirstName");
                list.Add("MiddleName");
                list.Add("LastName");
                list.Add("BranchName");
                if (list.Count > 0)
                {
                    cmb_ColPEmployyes.ValidValues.Add("-1", "[select one]");
                    foreach (var v in list)
                    {
                        cmb_ColPEmployyes.ValidValues.Add(Convert.ToString(v), Convert.ToString(v));
                    }
                    cmb_ColPEmployyes.Select(1, SAPbouiCOM.BoSearchKey.psk_Index);
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("FillColumnsPEmployees Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FilterRecordPEmployees()
        {
            try
            {
                string strColumnName = cmb_ColPEmployyes.Value;
                string strValue = txtSrchPEmployyes.Value.ToLower();
                SAPbouiCOM.Column col = grdEmpPost.Columns.Item("isSel");
                col.TitleObject.Caption = "";
                string cmbPeriodValue = cbPeriod.Value.Trim();
                if (cmbPeriodValue != "") //AR
                {
                    string strStatus = "";
                    if (optNPost.Selected)
                    {
                        strStatus = "SalaryStatus = 0 ";
                        IbtPost.Enabled = true;
                        IbtVoid.Enabled = true;
                    }
                    else
                    {
                        strStatus = "SalaryStatus = 1 ";
                        cbPayroll.Active = true;
                        IbtPost.Enabled = false;
                        IbtVoid.Enabled = false;
                    }

                    string strSql = @"
                        SELECT
	                        dbo.MstEmployee.EmpID, dbo.MstEmployee.SBOEmpCode, 
	                        dbo.MstEmployee.FirstName + ' ' + ISNULL(dbo.MstEmployee.MiddleName, '')+ ' ' + ISNULL(dbo.MstEmployee.LastName, '') AS empName,  
	                        dbo.MstEmployee.DepartmentName, dbo.MstEmployee.BranchName,dbo.MstEmployee.LocationName, dbo.TrnsSalaryProcessRegister.Id , 
	                        dbo.TrnsSalaryProcessRegister.SalaryStatus , isnull(JENum,'') as JENum   
                        FROM 
	                        dbo.MstEmployee 
	                        INNER JOIN dbo.TrnsSalaryProcessRegister ON dbo.MstEmployee.ID = dbo.TrnsSalaryProcessRegister.EmpID   
                        WHERE  
	                        " + strStatus + @"
	                        AND (dbo.TrnsSalaryProcessRegister.PayrollID = " + cbPayroll.Value.Trim().ToString() + @")
	                        AND (dbo.TrnsSalaryProcessRegister.PayrollPeriodID = " + cbPeriod.Value.Trim().ToString() + @" )";

                    if (strValue != null && strValue != "")
                    {
                        switch (strColumnName)
                        {
                            case "EmpID":
                                strSql += " AND dbo.MstEmployee.EmpID =   '" + strValue + "'";
                                break;
                            case "FirstName":
                                strSql += " AND dbo.MstEmployee.FirstName =   '" + strValue + "'";
                                break;
                            case "MiddleName":
                                strSql += " AND dbo.MstEmployee.MiddleName =   '" + strValue + "'";
                                break;
                            case "LastName":
                                strSql += " AND dbo.MstEmployee.LastName =   '" + strValue + "'";
                                break;
                            case "BranchName":
                                strSql += " AND dbo.MstEmployee.BranchName =   '" + strValue + "'";
                                break;
                            default:
                                break;
                        }
                        strSql += " ORDER BY dbo.MstEmployee.SortOrder ASC";
                        DataTable dtEmp = ds.getDataTable(strSql);
                        dtEmpsPost.Rows.Clear();
                        int i = 0;
                        foreach (DataRow dr in dtEmp.Rows)
                        {


                            dtEmpsPost.Rows.Add(1);
                            dtEmpsPost.SetValue("id", i, dr["ID"].ToString());
                            dtEmpsPost.SetValue("isSel", i, "N");
                            dtEmpsPost.SetValue("empId", i, dr["EmpID"].ToString());
                            dtEmpsPost.SetValue("empName", i, dr["empName"].ToString());
                            dtEmpsPost.SetValue("Dept", i, dr["DepartmentName"].ToString());
                            dtEmpsPost.SetValue("branch", i, dr["BranchName"].ToString());
                            dtEmpsPost.SetValue("Loc", i, dr["LocationName"].ToString());
                            dtEmpsPost.SetValue("empSboId", i, dr["JENum"].ToString());
                            dtEmpsPost.SetValue("Status", i, dr["SalaryStatus"].ToString() == "0" ? "N" : "Y");
                            i++;

                        }
                        grdEmpPost.LoadFromDataSource();

                    }
                    else
                    {
                        strSql += " ORDER BY dbo.MstEmployee.SortOrder ASC";
                        DataTable dtEmp = ds.getDataTable(strSql);
                        dtEmpsPost.Rows.Clear();
                        int i = 0;
                        foreach (DataRow dr in dtEmp.Rows)
                        {


                            dtEmpsPost.Rows.Add(1);
                            dtEmpsPost.SetValue("id", i, dr["ID"].ToString());
                            dtEmpsPost.SetValue("isSel", i, "N");
                            dtEmpsPost.SetValue("empId", i, dr["EmpID"].ToString());
                            dtEmpsPost.SetValue("empName", i, dr["empName"].ToString());
                            dtEmpsPost.SetValue("Dept", i, dr["DepartmentName"].ToString());
                            dtEmpsPost.SetValue("branch", i, dr["BranchName"].ToString());
                            dtEmpsPost.SetValue("Loc", i, dr["LocationName"].ToString());
                            dtEmpsPost.SetValue("empSboId", i, dr["JENum"].ToString());
                            dtEmpsPost.SetValue("Status", i, dr["SalaryStatus"].ToString() == "0" ? "N" : "Y");
                            i++;

                        }
                        grdEmpPost.LoadFromDataSource();

                    }
                }//AR
                grdEmpProc.LoadFromDataSource();
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("FilterRecordPEmployees : " + ex.Message);
            }
        }

        private void FilterRecordNPEmployees()
        {
            try
            {
                string strColumnName = cmb_ColNPEmployyes.Value;
                string strValue = txtSrchNPEmployyes.Value.ToLower();
                SAPbouiCOM.Column col = grdEmpProc.Columns.Item("isSel");
                col.TitleObject.Caption = "";

                string cmbPeriodValue = cbPeriod.Value.Trim();
                if (cmbPeriodValue != "") //AR
                {

                    string PeriodName = (from a in dbHrPayroll.CfgPeriodDates
                                         where a.ID.ToString() == cmbPeriodValue
                                         select a.PeriodName).FirstOrDefault() ?? "";


                    if (PeriodName == null) PeriodName = "";

                    string strSql = "SELECT     EmpID, ISNULL(SBOEmpCode,'') AS SBOEmpCode , ID, FirstName + ' ' + ISNULL(MiddleName, '')+ ' ' + LastName AS empName,  DepartmentName,BranchName, LocationName FROM dbo.MstEmployee";
                    strSql += " WHERE ISNULL(flgActive,0) <> 0 AND ISNULL(PayrollID, 0) = " + cbPayroll.Value.Trim();
                    strSql += " AND JoiningDate <= '" + PeriodEndDate.ToString("MM/dd/yyyy") + "'";
                    strSql += " AND ResignDate IS NULL";
                    strSql += " AND ID NOT IN (SELECT A1.EmpID FROM dbo.TrnsSalaryProcessRegister A1 WHERE A1.PayrollID = " + cbPayroll.Value.Trim() + " AND A1.PayrollPeriodID = " + cbPeriod.Value.Trim() + " )";
                    strSql += " AND ID NOT IN (SELECT A2.EmpID FROM dbo.TrnsSalaryProcessRegister A2 WHERE A2.PeriodName = '" + PeriodName + "')";
                    if (strValue != null && strValue != "")
                    {
                        switch (strColumnName)
                        {
                            case "EmpID":
                                strSql += " AND EmpID =    '" + strValue + "'";
                                break;
                            case "FirstName":
                                strSql += " AND FirstName =   '" + strValue + "'";
                                break;
                            case "MiddleName":
                                strSql += " AND MiddleName =   '" + strValue + "'";
                                break;
                            case "LastName":
                                strSql += " AND LastName =   '" + strValue + "'";
                                break;
                            case "BranchName":
                                strSql += " AND BranchName =   '" + strValue + "'";
                                break;
                            default:
                                break;
                        }
                        strSql += " ORDER BY dbo.MstEmployee.SortOrder ASC";
                        DataTable dtEmp = ds.getDataTable(strSql);
                        dtEmpsPr.Rows.Clear();
                        int i = 0;
                        foreach (DataRow dr in dtEmp.Rows)
                        {
                            dtEmpsPr.Rows.Add(1);
                            dtEmpsPr.SetValue("isSel", i, "N");
                            dtEmpsPr.SetValue("empId", i, dr["EmpID"].ToString());
                            dtEmpsPr.SetValue("empName", i, dr["empName"].ToString());
                            dtEmpsPr.SetValue("Dept", i, dr["DepartmentName"].ToString());
                            dtEmpsPr.SetValue("Loc", i, dr["LocationName"].ToString());
                            dtEmpsPr.SetValue("branch", i, dr["BranchName"].ToString());
                            i++;
                        }
                    }
                    else
                    {
                        strSql += " ORDER BY dbo.MstEmployee.SortOrder ASC";
                        DataTable dtEmp = ds.getDataTable(strSql);
                        dtEmpsPr.Rows.Clear();
                        int i = 0;
                        foreach (DataRow dr in dtEmp.Rows)
                        {
                            dtEmpsPr.Rows.Add(1);
                            dtEmpsPr.SetValue("isSel", i, "N");
                            dtEmpsPr.SetValue("empId", i, dr["EmpID"].ToString());
                            dtEmpsPr.SetValue("empName", i, dr["empName"].ToString());
                            dtEmpsPr.SetValue("Dept", i, dr["DepartmentName"].ToString());
                            dtEmpsPr.SetValue("Loc", i, dr["LocationName"].ToString());
                            dtEmpsPr.SetValue("branch", i, dr["BranchName"].ToString());
                            i++;
                        }
                    }
                }//AR
                grdEmpProc.LoadFromDataSource();
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("FilterRecordNPEmployees : " + ex.Message);
            }
        }

        private void fillCbs()
        {
            try
            {
                int i = 0;
                string selId = "0";
                //IEnumerable<CfgPayrollDefination> prs = from p in dbHrPayroll.CfgPayrollDefination select p;
                //foreach (CfgPayrollDefination pr in prs)
                //{
                //    cbPayroll.ValidValues.Add(pr.ID.ToString(), pr.PayrollName);

                //    i++;
                //}

                //cbPayroll.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                //FillPeriod(cbPayroll.Value);
                FillPayroll();

                IEnumerable<MstDepartment> depts = from p in dbHrPayroll.MstDepartment orderby p.DeptName ascending select p;
                cbDept.ValidValues.Add("0", "All");
                foreach (MstDepartment dept in depts)
                {
                    cbDept.ValidValues.Add(dept.ID.ToString(), dept.DeptName);

                }
                cbDept.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

                cbLoc.ValidValues.Add("0", "All");
                IEnumerable<MstLocation> locs = from p in dbHrPayroll.MstLocation orderby p.Description select p;

                foreach (MstLocation loc in locs)
                {
                    cbLoc.ValidValues.Add(loc.Id.ToString(), loc.Description);

                }
                cbLoc.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

                cmbBranch.ValidValues.Add("0", "All");
                IEnumerable<MstBranches> Branchs = from p in dbHrPayroll.MstBranches orderby p.Description select p;

                foreach (MstBranches Branch in Branchs)
                {
                    cmbBranch.ValidValues.Add(Branch.Id.ToString(), Branch.Description);

                }
                cmbBranch.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
            }
        }

        private void FillPayroll()
        {
            try
            {
                int i = 0;
                string strOut = string.Empty;
                string strSql = "SELECT \"U_PayrollType\" FROM \"OUSR\" WHERE \"USER_CODE\" = '" + oCompany.UserName + "'";
                SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                oRecSet.DoQuery(strSql);
                strOut = Convert.ToString(oRecSet.Fields.Item("U_PayrollType").Value);
                if (Program.systemInfo.FlgEmployeeFilter == true)
                {
                    if (strOut != null && strOut != "")
                    {
                        string strSql2 = sqlString.getSql("GetPayrollName", SearchKeyVal);
                        strSql2 = strSql2 + " where ID in (" + strOut + ")";
                        strSql2 += " ORDER BY ID Asc ";
                        System.Data.DataTable dt = ds.getDataTable(strSql2);
                        DataView dv = dt.DefaultView;
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            for (int l = 0; l < dt.Rows.Count; l++)
                            {
                                string strPayrollName = dt.Rows[l]["PayrollName"].ToString();
                                Int32 intPayrollID = Convert.ToInt32(dt.Rows[l]["ID"].ToString());
                                cbPayroll.ValidValues.Add(intPayrollID.ToString(), strPayrollName);

                            }
                        }
                        cbPayroll.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                        FillPeriod(cbPayroll.Value);
                    }
                    else
                    {
                        IEnumerable<CfgPayrollDefination> prs = from p in dbHrPayroll.CfgPayrollDefination select p;
                        foreach (CfgPayrollDefination pr in prs)
                        {
                            cbPayroll.ValidValues.Add(pr.ID.ToString(), pr.PayrollName);

                            i++;
                        }

                        cbPayroll.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                        FillPeriod(cbPayroll.Value);
                    }
                }
                else
                {
                    IEnumerable<CfgPayrollDefination> prs = from p in dbHrPayroll.CfgPayrollDefination select p;
                    foreach (CfgPayrollDefination pr in prs)
                    {
                        cbPayroll.ValidValues.Add(pr.ID.ToString(), pr.PayrollName);

                        i++;
                    }

                    cbPayroll.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    FillPeriod(cbPayroll.Value);
                }
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private void FillPeriod(string payroll)
        {
            try
            {
                dtPeriods.Rows.Clear();
                if (cbPeriod.ValidValues.Count > 0)
                {
                    int vcnt = cbPeriod.ValidValues.Count;
                    for (int k = vcnt - 1; k >= 0; k--)
                    {
                        cbPeriod.ValidValues.Remove(cbPeriod.ValidValues.Item(k).Value);
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
                        if (pd.FlgVisible == null ? false : (bool)pd.FlgVisible && pd.FlgLocked != true)
                        {
                            cbPeriod.ValidValues.Add(pd.ID.ToString(), pd.PeriodName.ToString());
                        }
                        count++;
                        if (!flgHit && count == 1)
                            selId = pd.ID.ToString();
                        //if (pd.StartDate <= DateTime.Now.Date && DateTime.Now.Date <= pd.EndDate)
                        //{
                        //    selId = pd.ID.ToString();
                        //}
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
                        cbPeriod.Select(selId);
                        //oForm.DataSources.UserDataSources.Item("cbPeriod").ValueEx = selId;
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
            }
        }

        private void fillJes()
        {
            try
            {
                string strSql = @"SELECT     ID, Memo, flgPosted, SBOJeNum, periodId
                            FROM         dbo.trnsJE where  periodId = '" + cbPeriod.Value.ToString().Trim() + @"'";



                DataTable dtJe = ds.getDataTable(strSql);
                dtJE.Rows.Clear();
                int i = 0;
                foreach (DataRow dr in dtJe.Rows)
                {


                    dtJE.Rows.Add(1);
                    dtJE.SetValue("jeNum", i, dr["ID"].ToString());
                    dtJE.SetValue("Descr", i, dr["Memo"].ToString());
                    dtJE.SetValue("EmpCount", i, "0");
                    dtJE.SetValue("SBOJe", i, dr["SBOJeNum"].ToString());
                    i++;

                }
                grdJe.LoadFromDataSource();
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
            }
        }

        private void getEmailEmployees()
        {
            try
            {
                if (!isFormLoad) return;
                if (grdEmpPost == null) return;
                SAPbouiCOM.Column col = grdEmpPost.Columns.Item("isSel");
                col.TitleObject.Caption = "";
                //DIHRMS.Custom.DataServices ds = new DIHRMS.Custom.DataServices(dbHrPayroll, Program.objHrmsUI.HRMSDbName, oCompany.UserName);
                string strStatus = "";
                //if (optNPost.Selected)
                //{
                //    strStatus = "SalaryStatus = 0 ";
                //    IbtPost.Enabled = true;
                //    IbtVoid.Enabled = true;
                //}
                //else
                //{

                //}
                if (optSendEmail.Selected)
                {
                    strStatus = "SalaryStatus = 1 ";
                    cbPayroll.Active = true;
                    IbtPost.Enabled = false;
                    IbtVoid.Enabled = false;
                }
                //string strSql = "SELECT     dbo.MstEmployee.EmpID, dbo.MstEmployee.SBOEmpCode, dbo.MstEmployee.FirstName + ' ' + ISNULL(dbo.MstEmployee.MiddleName, '')+ ' ' + ISNULL(dbo.MstEmployee.LastName, '') AS empName, ";
                //strSql += " dbo.MstEmployee.DepartmentName, dbo.MstEmployee.LocationName, dbo.TrnsSalaryProcessRegister.Id , dbo.TrnsSalaryProcessRegister.SalaryStatus , isnull(JENum,'') as JENum  ";
                //strSql += " FROM         dbo.MstEmployee INNER JOIN ";
                //strSql += "            dbo.TrnsSalaryProcessRegister ON dbo.MstEmployee.ID = dbo.TrnsSalaryProcessRegister.EmpID ";
                //strSql += "  WHERE  " + strStatus + "    (dbo.MstEmployee.PayrollID = " + cbPayroll.Value.ToString().Trim() + ") AND (dbo.TrnsSalaryProcessRegister.PayrollPeriodID = " + cbPeriod.Value.ToString().Trim() + " ) ";

                string strSql = @"
                        SELECT
	                        dbo.MstEmployee.EmpID, dbo.MstEmployee.SBOEmpCode, 
	                        dbo.MstEmployee.FirstName + ' ' + ISNULL(dbo.MstEmployee.MiddleName, '')+ ' ' + ISNULL(dbo.MstEmployee.LastName, '') AS empName,  
	                        dbo.MstEmployee.DepartmentName, dbo.MstEmployee.LocationName, dbo.TrnsSalaryProcessRegister.Id , 
	                        dbo.TrnsSalaryProcessRegister.SalaryStatus , isnull(JENum,'') as JENum   
                        FROM 
	                        dbo.MstEmployee 
	                        INNER JOIN dbo.TrnsSalaryProcessRegister ON dbo.MstEmployee.ID = dbo.TrnsSalaryProcessRegister.EmpID   
                        WHERE  
	                        " + strStatus + @"
	                        AND (dbo.TrnsSalaryProcessRegister.PayrollID = " + cbPayroll.Value.Trim().ToString() + @")
	                        AND (dbo.TrnsSalaryProcessRegister.PayrollPeriodID = " + cbPeriod.Value.Trim().ToString() + @" )  
                            AND Isnull(dbo.TrnsSalaryProcessRegister.flgEmailed,0)=1
                        ";

                if (cbDept.Value.ToString().Trim() != "0")
                {
                    strSql += " and departmentId = " + cbDept.Value.ToString();

                }
                if (cbLoc.Value.ToString().Trim() != "0")
                {
                    strSql += " and location = " + cbLoc.Value.ToString();
                }
                if (cmbBranch.Value.ToString().Trim() != "0")
                {
                    strSql += " and BranchID = " + cmbBranch.Value.ToString();
                }
                strSql += " ORDER BY dbo.MstEmployee.SortOrder ASC";
                DataTable dtEmp = ds.getDataTable(strSql);
                dtEmpsPost.Rows.Clear();
                int i = 0;
                foreach (DataRow dr in dtEmp.Rows)
                {


                    dtEmpsPost.Rows.Add(1);
                    dtEmpsPost.SetValue("id", i, dr["ID"].ToString());
                    dtEmpsPost.SetValue("isSel", i, "N");
                    dtEmpsPost.SetValue("empId", i, dr["EmpID"].ToString());
                    dtEmpsPost.SetValue("empName", i, dr["empName"].ToString());
                    dtEmpsPost.SetValue("Dept", i, dr["DepartmentName"].ToString());
                    dtEmpsPost.SetValue("Loc", i, dr["LocationName"].ToString());
                    dtEmpsPost.SetValue("empSboId", i, dr["JENum"].ToString());
                    dtEmpsPost.SetValue("Status", i, dr["SalaryStatus"].ToString() == "0" ? "N" : "Y");
                    i++;

                }
                grdEmpPost.LoadFromDataSource();
                fillJes();
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("getEmailEmployees :  " + ex.Message);
            }

        }

        private void getPEmployees()
        {
            try
            {
                if (!isFormLoad) return;
                if (grdEmpPost == null) return;
                SAPbouiCOM.Column col = grdEmpPost.Columns.Item("isSel");
                col.TitleObject.Caption = "";
                //DIHRMS.Custom.DataServices ds = new DIHRMS.Custom.DataServices(dbHrPayroll, Program.objHrmsUI.HRMSDbName, oCompany.UserName);
                string strStatus = "";
                if (optNPost.Selected)
                {
                    strStatus = "SalaryStatus = 0 ";
                    IbtPost.Enabled = true;
                    IbtVoid.Enabled = true;
                    IbtnSendEmail.Enabled = false;
                }
                else
                {
                    strStatus = "SalaryStatus = 1 ";
                    cbPayroll.Active = true;
                    IbtPost.Enabled = false;
                    IbtVoid.Enabled = false;
                    IbtnSendEmail.Enabled = true;
                }



                //string strSql = "SELECT     dbo.MstEmployee.EmpID, dbo.MstEmployee.SBOEmpCode, dbo.MstEmployee.FirstName + ' ' + ISNULL(dbo.MstEmployee.MiddleName, '')+ ' ' + ISNULL(dbo.MstEmployee.LastName, '') AS empName, ";
                //strSql += " dbo.MstEmployee.DepartmentName, dbo.MstEmployee.LocationName, dbo.TrnsSalaryProcessRegister.Id , dbo.TrnsSalaryProcessRegister.SalaryStatus , isnull(JENum,'') as JENum  ";
                //strSql += " FROM         dbo.MstEmployee INNER JOIN ";
                //strSql += "            dbo.TrnsSalaryProcessRegister ON dbo.MstEmployee.ID = dbo.TrnsSalaryProcessRegister.EmpID ";
                //strSql += "  WHERE  " + strStatus + "    (dbo.MstEmployee.PayrollID = " + cbPayroll.Value.ToString().Trim() + ") AND (dbo.TrnsSalaryProcessRegister.PayrollPeriodID = " + cbPeriod.Value.ToString().Trim() + " ) ";

                string strSql = @"
                        SELECT
	                        dbo.MstEmployee.EmpID, dbo.MstEmployee.SBOEmpCode, 
	                        dbo.MstEmployee.FirstName + ' ' + ISNULL(dbo.MstEmployee.MiddleName, '')+ ' ' + ISNULL(dbo.MstEmployee.LastName, '') AS empName,  
	                        dbo.MstEmployee.DepartmentName, dbo.MstEmployee.BranchName,dbo.MstEmployee.LocationName, dbo.TrnsSalaryProcessRegister.Id , 
	                        dbo.TrnsSalaryProcessRegister.SalaryStatus , isnull(JENum,'') as JENum   
                        FROM 
	                        dbo.MstEmployee 
	                        INNER JOIN dbo.TrnsSalaryProcessRegister ON dbo.MstEmployee.ID = dbo.TrnsSalaryProcessRegister.EmpID   
                        WHERE  
	                        " + strStatus + @"
	                        AND (dbo.TrnsSalaryProcessRegister.PayrollID = " + cbPayroll.Value.Trim().ToString() + @")
	                        AND (dbo.TrnsSalaryProcessRegister.PayrollPeriodID = " + cbPeriod.Value.Trim().ToString() + @" )  
                        ";

                if (cbDept.Value.ToString().Trim() != "0")
                {
                    strSql += " and departmentId = " + cbDept.Value.ToString();

                }
                if (cbLoc.Value.ToString().Trim() != "0")
                {
                    strSql += " and location = " + cbLoc.Value.ToString();
                }
                if (cmbBranch.Value.ToString().Trim() != "0")
                {
                    strSql += " and BranchID = " + cmbBranch.Value.ToString();
                }
                strSql += " ORDER BY dbo.MstEmployee.SortOrder ASC";
                DataTable dtEmp = ds.getDataTable(strSql);
                dtEmpsPost.Rows.Clear();
                int i = 0;
                foreach (DataRow dr in dtEmp.Rows)
                {


                    dtEmpsPost.Rows.Add(1);
                    dtEmpsPost.SetValue("id", i, dr["ID"].ToString());
                    dtEmpsPost.SetValue("isSel", i, "N");
                    dtEmpsPost.SetValue("empId", i, dr["EmpID"].ToString());
                    dtEmpsPost.SetValue("empName", i, dr["empName"].ToString());
                    dtEmpsPost.SetValue("Dept", i, dr["DepartmentName"].ToString());
                    dtEmpsPost.SetValue("branch", i, dr["BranchName"].ToString());
                    dtEmpsPost.SetValue("Loc", i, dr["LocationName"].ToString());
                    dtEmpsPost.SetValue("empSboId", i, dr["JENum"].ToString());
                    dtEmpsPost.SetValue("Status", i, dr["SalaryStatus"].ToString() == "0" ? "N" : "Y");
                    i++;

                }
                grdEmpPost.LoadFromDataSource();
                grdEmpPost.AutoResizeColumns();
                fillJes();
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("getPEmployees :  " + ex.Message);
            }

        }

        private void getNPEmployees()
        {
            try
            {
                if (!isFormLoad) return;
                SAPbouiCOM.Column col = grdEmpProc.Columns.Item("isSel");
                col.TitleObject.Caption = "";
                string cmbPeriodValue = cbPeriod.Value.Trim();

                string PeriodName = dbHrPayroll.CfgPeriodDates.Where(a => a.ID.ToString() == cmbPeriodValue).FirstOrDefault().PeriodName;

                if (PeriodName == null) PeriodName = "";

                string strSql = "SELECT     EmpID, ISNULL(SBOEmpCode,'') AS SBOEmpCode , ID, FirstName + ' ' + ISNULL(MiddleName, '')+ ' ' + LastName AS empName, DepartmentName,BranchName, LocationName FROM dbo.MstEmployee";
                strSql += " WHERE ISNULL(flgActive,0) <> 0 AND ISNULL(PayrollID, 0) = " + cbPayroll.Value.Trim();
                strSql += " AND JoiningDate <= '" + PeriodEndDate.ToString("MM/dd/yyyy") + "'";
                strSql += " AND ResignDate IS NULL";
                strSql += " AND ID NOT IN (SELECT A1.EmpID FROM dbo.TrnsSalaryProcessRegister A1 WHERE A1.PayrollID = " + cbPayroll.Value.Trim() + " AND A1.PayrollPeriodID = " + cbPeriod.Value.Trim() + " )";
                strSql += " AND ID NOT IN (SELECT A2.EmpID FROM dbo.TrnsSalaryProcessRegister A2 WHERE A2.PeriodName = '" + PeriodName + "')";
                if (cbDept.Value.ToString().Trim() != "0")
                {
                    strSql += " AND DepartmentID = " + cbDept.Value.ToString();
                }
                if (cbLoc.Value.ToString().Trim() != "0")
                {
                    strSql += " AND Location = " + cbLoc.Value.ToString();
                }
                if (cmbBranch.Value.ToString().Trim() != "0")
                {
                    strSql += " and BranchID = " + cmbBranch.Value.ToString();
                }
                strSql += " ORDER BY dbo.MstEmployee.SortOrder ASC";
                DataTable dtEmp = ds.getDataTable(strSql);

                dtEmpsPr.Rows.Clear();
                int i = 0;
                foreach (DataRow dr in dtEmp.Rows)
                {
                    dtEmpsPr.Rows.Add(1);
                    dtEmpsPr.SetValue("isSel", i, "N");
                    dtEmpsPr.SetValue("empId", i, dr["EmpID"].ToString());
                    dtEmpsPr.SetValue("empName", i, dr["empName"].ToString());
                    dtEmpsPr.SetValue("Dept", i, dr["DepartmentName"].ToString());
                    dtEmpsPr.SetValue("branch", i, dr["BranchName"].ToString());
                    dtEmpsPr.SetValue("Loc", i, dr["LocationName"].ToString());
                    i++;
                }
                grdEmpProc.LoadFromDataSource();
                grdEmpProc.AutoResizeColumns();
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("getNPEmployees : " + ex.Message);
            }
        }

        private void postSalar()
        {
            int confirm = oApplication.MessageBox("JE posting is irr-reversable. Are you sure you want to post salary? ", 3, "Yes", "No", "Cancel");
            if (confirm == 2 || confirm == 3) return;
            CfgPeriodDates payrollperiod = (from p in dbHrPayroll.CfgPeriodDates where p.ID.ToString() == cbPeriod.Value.ToString().Trim() select p).FirstOrDefault();

            int totalCnt = 0;
            string spIds = "0";
            grdEmpPost.FlushToDataSource();
            for (int i = 0; i < dtEmpsPost.Rows.Count; i++)
            {
                string sel = dtEmpsPost.GetValue("isSel", i);
                if (sel == "Y")
                {
                    if (totalCnt == 0)
                    {
                        spIds = dtEmpsPost.GetValue("id", i);
                    }
                    else
                    {
                        spIds += ", " + dtEmpsPost.GetValue("id", i);
                    }
                    totalCnt++;

                }
            }
            if (spIds == "0")
            {
                oApplication.SetStatusBarMessage("Select employees to post ");
            }
            SearchKeyVal.Clear();
            SearchKeyVal.Add("spIds", spIds);
            //string JeSql = sqlString.getSql("JEQuery", SearchKeyVal);
            string JeSql = sqlString.getSql("JEQueryMFM", SearchKeyVal);

            if (totalCnt > 0)
            {
                DataTable dtJeDetail = ds.getDataTable(JeSql);

                string errMsg = "";
                string strCode = "";
                string strName = "";
                foreach (DataRow dr in dtJeDetail.Rows)
                {
                    strCode = dr["AcctCode"].ToString();
                    strName = dr["AcctName"].ToString();
                    if (strCode == "Not Found")
                    {
                        errMsg = "GL Missing. Please confirm that GL Determination complete.";
                    }

                }
                if (errMsg != "")
                {
                    oApplication.SetStatusBarMessage(errMsg);
                    return;
                }
                TrnsJE je = new TrnsJE();
                je.CreateDt = DateTime.Now;
                je.FlgCanceled = false;
                je.FlgPosted = false;
                je.JEPostingDate = payrollperiod.EndDate;
                je.PayrollID = payrollperiod.CfgPayrollDefination.ID;
                je.PeriodID = payrollperiod.ID;
                je.Memo = " Payroll JE for period " + payrollperiod.PeriodName;

                foreach (DataRow dr in dtJeDetail.Rows)
                {
                    TrnsJEDetail jed = new TrnsJEDetail();
                    jed.AcctCode = dr["AcctCode"].ToString();
                    jed.AcctName = dr["AcctName"].ToString();
                    jed.Debit = Convert.ToDecimal(dr["Debit"].ToString());
                    jed.Credit = Convert.ToDecimal(dr["Credit"].ToString());
                    jed.CostCenter = dr["CostCenter"].ToString();
                    je.TrnsJEDetail.Add(jed);
                }
                dbHrPayroll.TrnsJE.InsertOnSubmit(je);
                dbHrPayroll.SubmitChanges();
                int jeNum = je.ID;

                for (int i = 0; i < dtEmpsPost.Rows.Count; i++)
                {
                    string sel = dtEmpsPost.GetValue("isSel", i);
                    string id = dtEmpsPost.GetValue("id", i);
                    if (sel == "Y")
                    {
                        string processId = dtEmpsPost.GetValue("id", i);
                        TrnsSalaryProcessRegister sp = (from p in dbHrPayroll.TrnsSalaryProcessRegister where p.Id.ToString() == processId select p).Single();
                        sp.JENum = jeNum;
                        sp.SalaryStatus = 1;

                    }

                }
                dbHrPayroll.SubmitChanges();

                getPEmployees();
            }

        }

        private void postSalaryDimensionWise()
        {
            int confirm = oApplication.MessageBox("JE posting is irr-reversable. Are you sure you want to post salary? ", 3, "Yes", "No", "Cancel");
            if (confirm == 2 || confirm == 3) return;
            CfgPeriodDates payrollperiod = (from p in dbHrPayroll.CfgPeriodDates where p.ID.ToString() == cbPeriod.Value.ToString().Trim() select p).FirstOrDefault();

            int totalCnt = 0;
            string spIds = "0";
            grdEmpPost.FlushToDataSource();
            for (int i = 0; i < dtEmpsPost.Rows.Count; i++)
            {
                string sel = dtEmpsPost.GetValue("isSel", i);
                if (sel == "Y")
                {
                    if (totalCnt == 0)
                    {
                        spIds = dtEmpsPost.GetValue("id", i);
                    }
                    else
                    {
                        spIds += ", " + dtEmpsPost.GetValue("id", i);
                    }
                    totalCnt++;

                }
            }
            if (spIds == "0")
            {
                oApplication.SetStatusBarMessage("Select employees to post ");
            }
            SearchKeyVal.Clear();
            SearchKeyVal.Add("spIds", spIds);
            string JeSql = sqlString.getSql("JEQueryDimension", SearchKeyVal);

            if (totalCnt > 0)
            {
                DataTable dtJeDetail = ds.getDataTable(JeSql);

                string errMsg = "";
                string strCode = "";
                string strName = "";
                foreach (DataRow dr in dtJeDetail.Rows)
                {
                    strCode = dr["AcctCode"].ToString();
                    strName = dr["AcctName"].ToString();
                    if (strCode == "Not Found")
                    {
                        errMsg = "GL Missing. Please confirm that GL Determination complete.";
                    }

                }
                if (errMsg != "")
                {
                    oApplication.SetStatusBarMessage(errMsg);
                    return;
                }
                TrnsJE je = new TrnsJE();
                je.CreateDt = DateTime.Now;
                je.FlgCanceled = false;
                je.FlgPosted = false;
                je.JEPostingDate = payrollperiod.EndDate;
                je.PayrollID = payrollperiod.CfgPayrollDefination.ID;
                je.PeriodID = payrollperiod.ID;
                je.Memo = " Payroll JE for period " + payrollperiod.PeriodName;

                foreach (DataRow dr in dtJeDetail.Rows)
                {
                    TrnsJEDetail jed = new TrnsJEDetail();
                    jed.AcctCode = dr["AcctCode"].ToString();
                    jed.AcctName = dr["AcctName"].ToString();
                    jed.Debit = Convert.ToDecimal(dr["Debit"].ToString());
                    jed.Credit = Convert.ToDecimal(dr["Credit"].ToString());
                    jed.CostCenter = "";
                    jed.BranchName = "";
                    jed.Dimension1 = dr["Dimension1"].ToString();
                    jed.Dimension2 = dr["Dimension2"].ToString();
                    jed.Dimension3 = dr["Dimension3"].ToString();
                    jed.Dimension4 = dr["Dimension4"].ToString();
                    jed.Dimension5 = dr["Dimension5"].ToString();
                    jed.FCurrency = dr["EmpCurr"].ToString();
                    je.TrnsJEDetail.Add(jed);
                }
                dbHrPayroll.TrnsJE.InsertOnSubmit(je);
                dbHrPayroll.SubmitChanges();
                int jeNum = je.ID;

                for (int i = 0; i < dtEmpsPost.Rows.Count; i++)
                {
                    string sel = dtEmpsPost.GetValue("isSel", i);
                    string id = dtEmpsPost.GetValue("id", i);
                    if (sel == "Y")
                    {
                        string processId = dtEmpsPost.GetValue("id", i);
                        TrnsSalaryProcessRegister sp = (from p in dbHrPayroll.TrnsSalaryProcessRegister where p.Id.ToString() == processId select p).Single();
                        sp.JENum = jeNum;
                        sp.SalaryStatus = 1;

                    }

                }
                dbHrPayroll.SubmitChanges();

                getPEmployees();
            }

        }

        private void ProcessSalary()
        {
            string strProcessing = "";
            try
            {
                IEnumerable<MstEmployee> emps = from p in dbHrPayroll.MstEmployee select p;
                //dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, emps);
                Hashtable elementGls = new Hashtable();
                grdEmpProc.FlushToDataSource();

                int totalCnt = 0;
                for (int i = 0; i < dtEmpsPr.Rows.Count; i++)
                {
                    string sel = dtEmpsPr.GetValue("isSel", i);
                    if (sel == "Y")
                    {
                        totalCnt++;
                    }
                }

                SAPbouiCOM.ProgressBar prog = oApplication.StatusBar.CreateProgressBar("Processing Salary", totalCnt, false);
                prog.Value = 0;
                CfgPayrollDefination payroll = (from p in dbHrPayroll.CfgPayrollDefination where p.ID.ToString() == cbPayroll.Value.ToString() select p).FirstOrDefault();
                CfgPeriodDates payrollperiod = (from p in dbHrPayroll.CfgPeriodDates where p.ID.ToString() == cbPeriod.Value.ToString() select p).FirstOrDefault();
                int periodDays = 0;
                periodDays = Convert.ToInt16(payroll.WorkDays);
                decimal empBasicSalary = 0;
                decimal empGrossSalary = 0;

                try
                {
                    for (int i = 0; i < dtEmpsPr.Rows.Count; i++)
                    {

                        decimal amnt = 0.0M;



                        string sel = dtEmpsPr.GetValue("isSel", i);
                        if (sel == "Y")
                        {
                            prog.Value += 1;

                            string empid = Convert.ToString(dtEmpsPr.GetValue("empId", i));
                            MstEmployee emp = (from p in dbHrPayroll.MstEmployee where p.EmpID.ToString() == empid select p).FirstOrDefault();
                            MstGLDetermination glDetr = ds.getEmpGl(emp);
                            if (glDetr == null)
                            {
                                oApplication.StatusBar.SetText("EmpCode : " + emp.EmpID + " Doesn't have GL determination defined in respected Location or Deparment.", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                continue;
                            }
                            empBasicSalary = (decimal)emp.BasicSalary;
                            decimal spTaxbleAmnt = 0.00M;
                            decimal spTaxableAmntOT = 0.00M;
                            decimal spTaxableAmntLWOP = 0.00M;
                            decimal DaysCnt = 0;
                            decimal payDays = 0.00M;
                            decimal leaveDays = 0.00M;
                            decimal monthDays = 0.00M;
                            decimal nonRecurringTaxable = 0.00M;
                            decimal payRatio = 1.00M;
                            decimal payRatioWithLeaves = 1.00M;

                            DaysCnt = ds.getDaysCnt(emp, payrollperiod, out payDays, out leaveDays, out monthDays);
                            decimal employeeRemainingSalary = 0.00M;
                            payRatio = payDays / monthDays;
                            payRatioWithLeaves = (payDays - leaveDays) / monthDays;

                            if (!string.IsNullOrEmpty(emp.EmployeeContractType) && emp.EmployeeContractType == "DWGS")
                            {
                                empGrossSalary = ds.getEmpGross(emp, payrollperiod.ID);
                                empBasicSalary = empGrossSalary;
                            }
                            else
                            {
                                empGrossSalary = ds.getEmpGross(emp);
                            }
                            prog.Text = "(" + prog.Value.ToString() + " of " + totalCnt.ToString() + " ) Processing Salary --> " + emp.FirstName + " " + emp.LastName;
                            strProcessing = "Error in Processing Salary --> " + emp.EmpID + ":" + emp.FirstName + " " + emp.LastName + " (" + i.ToString() + " of " + totalCnt.ToString() + " ) ";
                            Application.DoEvents();

                            try
                            {
                                employeeRemainingSalary = Math.Round((decimal)empBasicSalary * payRatio, 0);
                            }
                            catch { }
                            TrnsSalaryProcessRegister reg = new TrnsSalaryProcessRegister();
                            reg.MstEmployee = emp;
                            reg.CfgPayrollDefination = payroll;
                            reg.CfgPeriodDates = payrollperiod;
                            reg.EmpBasic = employeeRemainingSalary;//Math.Round(Convert.ToDecimal(empBasicSalary * payRatio), 0);
                            reg.EmpGross = empGrossSalary;
                            if (emp.MstDepartment != null)
                            {
                                reg.EmpDepartment = emp.MstDepartment.DeptName;
                            }
                            else
                            {
                                reg.EmpDepartment = "";
                            }
                            if (emp.MstDesignation != null)
                            {
                                reg.EmpDesignation = emp.MstDesignation.Description;
                            }
                            else
                            {
                                reg.EmpDesignation = "";
                            }
                            if (emp.MstLocation != null)
                            {
                                reg.EmpLocation = emp.MstLocation.Description;
                            }
                            else
                            {
                                reg.EmpLocation = "";
                            }
                            if (emp.MstBranches != null)
                            {
                                reg.EmpBranch = emp.MstBranches.Description;
                            }
                            else
                            {
                                reg.EmpBranch = "";
                            }
                            if (emp.MstPosition != null)
                            {
                                reg.EmpPosition = emp.MstPosition.Description;
                            }
                            else
                            {
                                reg.EmpPosition = "";
                            }
                            if (string.IsNullOrEmpty(emp.JobTitle))
                            {
                                reg.EmpJobTitle = "";
                            }
                            else
                            {
                                MstJobTitle oTitle = (from a in dbHrPayroll.MstJobTitle where a.Id.ToString() == emp.JobTitle select a).FirstOrDefault();
                                if (oTitle != null)
                                {
                                    reg.EmpJobTitle = oTitle.Description;
                                }
                            }
                            reg.CreateDate = DateTime.Now;
                            reg.UpdateDate = DateTime.Now;
                            reg.UserId = oCompany.UserName;
                            reg.UpdatedBy = oCompany.UserName;
                            reg.PeriodName = payrollperiod.PeriodName;
                            reg.PayrollName = payroll.PayrollName;
                            reg.EmpName = emp.FirstName + " " + emp.LastName;
                            //reg.DaysPaid = Convert.ToInt16( payDays);
                            reg.DaysPaid = Convert.ToDecimal(DaysCnt);
                            reg.MonthDays = Convert.ToInt32(monthDays);

                            /// Basic Salary ////
                            /// ************////
                            TrnsSalaryProcessRegisterDetail spdHeadRow = new TrnsSalaryProcessRegisterDetail();
                            spdHeadRow.LineType = "BS";
                            spdHeadRow.LineSubType = "Basic Salary";
                            spdHeadRow.LineValue = Math.Round(employeeRemainingSalary, 0);
                            spdHeadRow.LineMemo = "Basic Salary ";
                            spdHeadRow.DebitAccount = glDetr.BasicSalary;
                            spdHeadRow.CreditAccount = glDetr.BSPayable;
                            spdHeadRow.DebitAccountName = Program.objHrmsUI.getAcctName(glDetr.BasicSalary);
                            spdHeadRow.CreditAccountName = Program.objHrmsUI.getAcctName(glDetr.BSPayable);
                            spdHeadRow.LineBaseEntry = emp.ID;
                            spdHeadRow.BaseValueCalculatedOn = employeeRemainingSalary;
                            spdHeadRow.BaseValue = employeeRemainingSalary;
                            spdHeadRow.BaseValueType = "FIX";
                            spdHeadRow.CreateDate = DateTime.Now;
                            spdHeadRow.UpdateDate = DateTime.Now;
                            spdHeadRow.UserId = oCompany.UserName;
                            spdHeadRow.UpdatedBy = oCompany.UserName;
                            spdHeadRow.NoOfDay = Convert.ToDecimal(DaysCnt);
                            spdHeadRow.TaxableAmount = employeeRemainingSalary;
                            spTaxbleAmnt += employeeRemainingSalary;
                            // employeeRemainingSalary += (decimal)spdHeadRow.LineValue;
                            reg.TrnsSalaryProcessRegisterDetail.Add(spdHeadRow);





                            //* AbsentDeductions,Reimbursement

                            //////Absents ////
                            //**************////
                            decimal leaveCnt = 0.00M;
                            //DataTable dtAbsentDeduction = ds.salaryProcessingAbsents(emp, payrollperiod, (decimal)reg.EmpGross, out leaveCnt);

                            DataTable dtAbsentDeduction = ds.salaryProcessingAbsents(emp, payrollperiod, (decimal)reg.EmpGross, out leaveCnt, glDetr);
                            foreach (DataRow dr in dtAbsentDeduction.Rows)
                            {
                                TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                                spdetail.LineType = dr["LineType"].ToString();
                                spdetail.LineSubType = dr["LineSubType"].ToString();
                                spdetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                                spdetail.LineMemo = dr["LineMemo"].ToString();
                                spdetail.DebitAccount = dr["DebitAccount"].ToString();
                                spdetail.CreditAccount = dr["CreditAccount"].ToString();
                                spdetail.DebitAccountName = dr["DebitAccountName"].ToString();
                                spdetail.CreditAccountName = dr["CreditAccountName"].ToString();
                                spdetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
                                spdetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
                                spdetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
                                spdetail.BaseValueType = dr["BaseValueType"].ToString(); ;
                                spdetail.CreateDate = DateTime.Now;
                                spdetail.UpdateDate = DateTime.Now;
                                spdetail.UserId = oCompany.UserName;
                                spdetail.UpdatedBy = oCompany.UserName;
                                spdetail.TaxableAmount = Convert.ToDecimal(dr["TaxbleAmnt"]);
                                spTaxbleAmnt += Convert.ToDecimal(dr["TaxbleAmnt"]);
                                employeeRemainingSalary += (decimal)spdetail.LineValue;
                                spTaxableAmntLWOP += Convert.ToDecimal(dr["TaxbleAmnt"]);
                                spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
                                reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
                            }

                            //DaysCnt -= leaveCnt;
                            //* End of Leave Deductions


                            //* Payroll elements assigned to employee ***Employee Elements ****** 
                            //*******************************************************************

                            DataTable dtSalPrlElements = ds.salaryProcessingElements(emp, payrollperiod, DaysCnt, empGrossSalary, glDetr, payRatio, payRatioWithLeaves, monthDays - leaveCnt, monthDays);
                            foreach (DataRow dr in dtSalPrlElements.Rows)
                            {
                                if (Math.Round(Convert.ToDecimal(dr["LineValue"]), 0) != 0)
                                {
                                    TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                                    spdetail.LineType = dr["LineType"].ToString();
                                    spdetail.LineSubType = dr["LineSubType"].ToString();
                                    spdetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                                    spdetail.LineMemo = dr["LineMemo"].ToString();
                                    spdetail.DebitAccount = dr["DebitAccount"].ToString();
                                    spdetail.CreditAccount = dr["CreditAccount"].ToString();
                                    spdetail.DebitAccountName = dr["DebitAccountName"].ToString();
                                    spdetail.CreditAccountName = dr["CreditAccountName"].ToString();
                                    spdetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
                                    spdetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
                                    spdetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
                                    spdetail.BaseValueType = dr["BaseValueType"].ToString(); ;
                                    spdetail.CreateDate = DateTime.Now;
                                    spdetail.UpdateDate = DateTime.Now;
                                    spdetail.UserId = oCompany.UserName;
                                    spdetail.UpdatedBy = oCompany.UserName;
                                    spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
                                    spdetail.TaxableAmount = Convert.ToDecimal(dr["TaxbleAmnt"]);
                                    spTaxbleAmnt += Convert.ToDecimal(dr["TaxbleAmnt"]);
                                    nonRecurringTaxable += Convert.ToDecimal(dr["NRTaxbleAmnt"]);
                                    employeeRemainingSalary += (decimal)spdetail.LineValue;
                                    reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
                                }
                            }
                            //******************** End of Elements *********************************



                            //////Over time ////
                            //**************////
                            Int32 otmin = 0;
                            DataTable dtSalOverTimes = ds.salaryProcessingOvertimes(emp, payrollperiod, empGrossSalary, out otmin);

                            //Code modified by Zeeshan

                            foreach (DataRow dr in dtSalOverTimes.Rows)
                            {
                                TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                                spdetail.LineType = dr["LineType"].ToString();
                                spdetail.LineSubType = dr["LineSubType"].ToString();
                                spdetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                                spdetail.LineMemo = dr["LineMemo"].ToString();
                                spdetail.DebitAccount = dr["DebitAccount"].ToString();
                                spdetail.CreditAccount = dr["CreditAccount"].ToString();
                                spdetail.DebitAccountName = dr["DebitAccountName"].ToString();
                                spdetail.CreditAccountName = dr["CreditAccountName"].ToString();
                                spdetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
                                spdetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
                                spdetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
                                spdetail.BaseValueType = dr["BaseValueType"].ToString(); ;
                                spdetail.CreateDate = DateTime.Now;
                                spdetail.UpdateDate = DateTime.Now;
                                spdetail.UserId = oCompany.UserName;
                                spdetail.UpdatedBy = oCompany.UserName;
                                spdetail.TaxableAmount = Convert.ToDecimal(dr["TaxbleAmnt"]);
                                spTaxbleAmnt += Convert.ToDecimal(dr["TaxbleAmnt"]);
                                spTaxableAmntOT += Convert.ToDecimal(dr["TaxbleAmnt"]);
                                employeeRemainingSalary += (decimal)spdetail.LineValue;
                                spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
                                reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
                            }



                            // * ************Advance Recovery Processing **************
                            //*******************************************************
                            DataTable dtAdvance = ds.salaryProcessingAdvance(emp, employeeRemainingSalary, payrollperiod);

                            foreach (DataRow dr in dtAdvance.Rows)
                            {
                                TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                                spdetail.LineType = dr["LineType"].ToString();
                                spdetail.LineSubType = dr["LineSubType"].ToString();
                                spdetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                                spdetail.LineMemo = dr["LineMemo"].ToString();
                                spdetail.DebitAccount = dr["DebitAccount"].ToString();
                                spdetail.CreditAccount = dr["CreditAccount"].ToString();
                                spdetail.DebitAccountName = dr["DebitAccountName"].ToString();
                                spdetail.CreditAccountName = dr["CreditAccountName"].ToString();
                                spdetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
                                spdetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
                                spdetail.A1Indicators = Convert.ToString(dr["Indicator"]);
                                spdetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
                                spdetail.BaseValueType = dr["BaseValueType"].ToString(); ;
                                spdetail.CreateDate = DateTime.Now;
                                spdetail.UpdateDate = DateTime.Now;
                                spdetail.UserId = oCompany.UserName;
                                spdetail.UpdatedBy = oCompany.UserName;
                                spdetail.TaxableAmount = 0.00M;
                                employeeRemainingSalary += (decimal)spdetail.LineValue;


                                spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
                                reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
                            }



                            // * ************Loan Recovery Processing **************

                            DataTable dtLoands = ds.salaryProcessingLoans(emp, employeeRemainingSalary, payrollperiod);

                            foreach (DataRow dr in dtLoands.Rows)
                            {
                                TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                                spdetail.LineType = dr["LineType"].ToString();
                                spdetail.LineSubType = dr["LineSubType"].ToString();
                                spdetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                                spdetail.LineMemo = dr["LineMemo"].ToString();
                                spdetail.DebitAccount = dr["DebitAccount"].ToString();
                                spdetail.CreditAccount = dr["CreditAccount"].ToString();
                                spdetail.DebitAccountName = dr["DebitAccountName"].ToString();
                                spdetail.CreditAccountName = dr["CreditAccountName"].ToString();
                                spdetail.A1Indicators = Convert.ToString(dr["Indicator"]);
                                spdetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
                                spdetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
                                spdetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
                                spdetail.BaseValueType = dr["BaseValueType"].ToString(); ;
                                spdetail.CreateDate = DateTime.Now;
                                spdetail.UpdateDate = DateTime.Now;
                                spdetail.UserId = oCompany.UserName;
                                spdetail.UpdatedBy = oCompany.UserName;
                                spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
                                spdetail.TaxableAmount = 0.00M;
                                employeeRemainingSalary += (decimal)spdetail.LineValue;
                                reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
                            }

                            reg.EmpTaxblTotal = spTaxbleAmnt;
                            // * ************TAX**************
                            Decimal QuaterlyTaxValueReturn = 0.0M;
                            if (Program.systemInfo.TaxSetup == true && emp.FlgTax == true)
                            {
                                //decimal TotalTax = ds.getEmployeeTaxAmount(payrollperiod, emp, spTaxableAmntOT, spTaxableAmntLWOP, spTaxbleAmnt, nonRecurringTaxable);
                                decimal TotalTax = ds.getEmployeeTaxAmountIncentivePayment(payrollperiod, emp, spTaxableAmntOT, spTaxableAmntLWOP, spTaxbleAmnt, nonRecurringTaxable, empGrossSalary, payRatio, out QuaterlyTaxValueReturn);
                                if (TotalTax >= 0)
                                {
                                    reg.EmpTotalTax = TotalTax;

                                    TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                                    spdetail.LineType = "Tax";
                                    spdetail.LineSubType = "Tax";
                                    spdetail.LineValue = -Math.Round(TotalTax, 0);
                                    spdetail.LineMemo = "Tax Deduction";
                                    spdetail.DebitAccount = glDetr.IncomeTaxExpense;
                                    spdetail.CreditAccount = glDetr.IncomeTaxPayable;
                                    spdetail.DebitAccountName = Program.objHrmsUI.getAcctName(glDetr.IncomeTaxExpense);
                                    spdetail.CreditAccountName = Program.objHrmsUI.getAcctName(glDetr.IncomeTaxPayable);
                                    spdetail.LineBaseEntry = 0;
                                    spdetail.BaseValueCalculatedOn = spTaxbleAmnt + QuaterlyTaxValueReturn;
                                    spdetail.BaseValue = spTaxbleAmnt + QuaterlyTaxValueReturn;
                                    spdetail.BaseValueType = "FIX";
                                    spdetail.CreateDate = DateTime.Now;
                                    spdetail.UpdateDate = DateTime.Now;
                                    spdetail.UserId = oCompany.UserName;
                                    spdetail.UpdatedBy = oCompany.UserName;
                                    spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
                                    spdetail.TaxableAmount = 0.00M;
                                    reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
                                }
                            }
                            spTaxbleAmnt = spTaxbleAmnt + QuaterlyTaxValueReturn;
                            //reg.EmpTaxblTotal = spTaxbleAmnt;

                            //************************************************
                            //********** Gratuity Calculations ***************

                            if (emp.CfgPayrollDefination.FlgGratuity == true)
                            {
                                int gratCnt = (from p in dbHrPayroll.MstGratuity where p.Id == emp.CfgPayrollDefination.GratuityID select p).Count();
                                if (gratCnt > 0)
                                {
                                    MstGratuity empGrat = (from p in dbHrPayroll.MstGratuity where p.Id == emp.CfgPayrollDefination.GratuityID select p).FirstOrDefault();

                                    try
                                    {
                                        int FromYr = Convert.ToInt16(empGrat.YearFrom) * 365;

                                        if ((Convert.ToDateTime(payrollperiod.StartDate) - Convert.ToDateTime(emp.JoiningDate)).Days > FromYr)
                                        {
                                            decimal gratProvision = 0.00M;
                                            decimal basedOnAmont = 0.00M;
                                            if (empGrat.BasedOn == "0")
                                            {
                                                basedOnAmont = empBasicSalary;
                                            }
                                            else
                                            {
                                                basedOnAmont = empGrossSalary;
                                            }

                                            gratProvision = (basedOnAmont * (decimal)empGrat.Factor / 100) / 12;
                                            TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                                            spdetail.LineType = "Element";
                                            spdetail.LineSubType = "Empr Cont";
                                            spdetail.LineValue = Math.Round(gratProvision, 0);
                                            spdetail.LineMemo = "Gratuity";
                                            spdetail.DebitAccount = glDetr.GratuityExpense;
                                            spdetail.CreditAccount = glDetr.GratuityPayable;
                                            spdetail.DebitAccountName = Program.objHrmsUI.getAcctName(glDetr.GratuityExpense);
                                            spdetail.CreditAccountName = Program.objHrmsUI.getAcctName(glDetr.GratuityPayable);
                                            spdetail.LineBaseEntry = 0;
                                            spdetail.BaseValueCalculatedOn = empBasicSalary;
                                            spdetail.BaseValue = empBasicSalary;
                                            spdetail.BaseValueType = "FIX";
                                            spdetail.CreateDate = DateTime.Now;
                                            spdetail.UpdateDate = DateTime.Now;
                                            spdetail.UserId = oCompany.UserName;
                                            spdetail.UpdatedBy = oCompany.UserName;
                                            spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
                                            spdetail.TaxableAmount = 0.00M;
                                            reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                            }
                            reg.SalaryStatus = 0;//Salary Processed
                            if (emp.PaymentMode == "HOLD")
                            {
                                reg.FlgHoldPayment = true;
                            }
                            dbHrPayroll.TrnsSalaryProcessRegister.InsertOnSubmit(reg);
                        }

                    }
                    dbHrPayroll.SubmitChanges();
                }
                catch (Exception ex)
                {
                    oApplication.SetStatusBarMessage(strProcessing + ":" + ex.Message);
                }
                prog.Stop();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(prog);

                prog = null;
                getNPEmployees();
                getPEmployees();
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage(ex.Message);
            }
        }

        private void ProcessSalaryMFM()
        {
            string strProcessing = string.Empty;
            try
            {
                using (dbHRMS oDBPrivate = new dbHRMS(Program.ConStrHRMS))
                {
                    Hashtable elementGls = new Hashtable();
                    List<string> oSelectedEmployee = new List<string>();
                    grdEmpProc.FlushToDataSource();
                    int totalCnt = 0;
                    for (int i = 0; i < dtEmpsPr.Rows.Count; i++)
                    {
                        string sel = dtEmpsPr.GetValue("isSel", i);
                        if (sel == "Y")
                        {
                            string empid = Convert.ToString(dtEmpsPr.GetValue("empId", i));
                            oSelectedEmployee.Add(empid);
                        }
                    }
                    totalCnt = oSelectedEmployee.Count;
                    SAPbouiCOM.ProgressBar prog = oApplication.StatusBar.CreateProgressBar("Processing Salary", totalCnt, false);
                    prog.Value = 0;
                    CfgPayrollDefination oPayroll = (from p in oDBPrivate.CfgPayrollDefination where p.ID.ToString() == cbPayroll.Value.ToString() select p).FirstOrDefault();
                    CfgPeriodDates oPayrollPeriod = (from p in oDBPrivate.CfgPeriodDates where p.ID.ToString() == cbPeriod.Value.ToString() select p).FirstOrDefault();
                    int periodDays = 0;
                    periodDays = Convert.ToInt16(oPayroll.WorkDays);
                    decimal empBasicSalary = 0;
                    decimal empGrossSalary = 0;
                    decimal Percent = 1;
                    #region Processing Details
                    try
                    {
                        //for (int i = 0; i < dtEmpsPr.Rows.Count; i++)
                        int i = 0;
                        foreach (var One in oSelectedEmployee)
                        {
                            decimal amnt = 0.0M;
                            #region Head
                            prog.Value += 1;

                            MstEmployee emp = (from p in oDBPrivate.MstEmployee where p.EmpID.ToString() == One select p).FirstOrDefault();

                            //Anas ka kaam
                            if (true)
                            {
                                try
                                {
                                    Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                                    string SQL = "SELECT TOP 1 \"percentage\" FROM \"FinalEval3\" WHERE \"emp_id\" = '" + emp.EmpID + "' AND \"year\" = " + Convert.ToDateTime(oPayrollPeriod.StartDate).Year + " AND \"monthid\" = " + Convert.ToDateTime(oPayrollPeriod.StartDate).Month;
                                    //string SQL = "SELECT TOP 1 \"percentage\" FROM \"FinalEval3\" WHERE \"emp_id\" = '" + emp.EmpID + "' AND \"year\" = " + Convert.ToDateTime(oPayrollPeriod.StartDate).Year + " AND \"ActualMonth\" = " + Convert.ToDateTime(oPayrollPeriod.StartDate).Month;
                                    logger(SQL);
                                    oRecSet.DoQuery(SQL);
                                    if (oRecSet.RecordCount > 0)
                                    {
                                        Percent = Convert.ToDecimal(oRecSet.Fields.Item(0).Value) / 100;
                                        logger("pecentage value: " + Percent.ToString());
                                    }
                                }
                                catch(Exception ex)
                                {
                                    logger(ex);
                                }
                            }
                            //End of Anas ka kaam
                            MstGLDetermination glDetr = ds.getEmpGl(emp);
                            if (glDetr == null)
                            {
                                oApplication.StatusBar.SetText("EmpCode : " + emp.EmpID + " Doesn't have GL determination defined in respected Location or Deparment.", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                continue;
                            }
                            //Processing Based on Attendance.
                            //if ((Boolean)Program.systemInfo.AttendanceSystem && (Boolean)Program.systemInfo.FlgProcessingOnAttendance)
                            Int32 PeriodDayCount = 0;
                            if (Program.systemInfo.AttendanceSystem == null ? false : Program.systemInfo.AttendanceSystem == true && Program.systemInfo.FlgProcessingOnAttendance == null ? false : Program.systemInfo.FlgProcessingOnAttendance == true)
                            {
                                Int32 PostedDayCount = (from a in oDBPrivate.TrnsAttendanceRegister
                                                        where a.EmpID == emp.ID
                                                        && a.FlgPosted == true && a.PeriodID == oPayrollPeriod.ID
                                                        select a).Count();

                                if (emp.JoiningDate >= oPayrollPeriod.StartDate && emp.JoiningDate <= oPayrollPeriod.EndDate)
                                {
                                    PeriodDayCount = (Convert.ToDateTime(oPayrollPeriod.EndDate) - Convert.ToDateTime(emp.JoiningDate)).Days + 1;
                                }
                                else
                                {
                                    PeriodDayCount = (Convert.ToDateTime(oPayrollPeriod.EndDate) - Convert.ToDateTime(oPayrollPeriod.StartDate)).Days + 1;
                                }
                                if (!string.IsNullOrEmpty(emp.EmployeeContractType) && emp.EmployeeContractType != "DWGS")
                                {
                                    if (PostedDayCount != PeriodDayCount)
                                    {
                                        MsgWarning("Attendance not posted for Employee : " + emp.EmpID);
                                        continue;
                                    }
                                }
                            }
                            //End of Code
                            #region Half Monthly Basic Salary
                            if (oPayrollPeriod.CfgPayrollDefination.PayrollType.Trim() == "HMNT")
                            {
                                DateTime dtGetMonthDays;
                                Int32 intGetMonthDays;
                                decimal decPerDayBasicSalary = 0;
                                dtGetMonthDays = Convert.ToDateTime(oPayrollPeriod.StartDate);
                                intGetMonthDays = DateTime.DaysInMonth(dtGetMonthDays.Year, dtGetMonthDays.Month);
                               
                                if (CompanyName.ToLower() == "pakola")
                                {
                                    empBasicSalary = (decimal)emp.BasicSalary / 2;
                                }
                                else
                                {
                                    if (emp.JoiningDate >= oPayrollPeriod.StartDate && emp.JoiningDate <= oPayrollPeriod.EndDate)
                                    {
                                        PeriodDayCount = (Convert.ToDateTime(oPayrollPeriod.EndDate) - Convert.ToDateTime(emp.JoiningDate)).Days + 1;
                                        decPerDayBasicSalary = (decimal)emp.BasicSalary / intGetMonthDays;
                                        empBasicSalary = decPerDayBasicSalary * PeriodDayCount;                                        
                                        
                                    }
                                    else
                                    {
                                        PeriodDayCount = (Convert.ToDateTime(oPayrollPeriod.EndDate) - Convert.ToDateTime(oPayrollPeriod.StartDate)).Days + 1;
                                        decPerDayBasicSalary = (decimal)emp.BasicSalary / intGetMonthDays;
                                        empBasicSalary = decPerDayBasicSalary * PeriodDayCount;
                                    }
                                }

                            }
                            else
                            {
                                empBasicSalary = (decimal)emp.BasicSalary;
                            }
                            #endregion
                            //empBasicSalary = (decimal)emp.BasicSalary;
                            decimal spTaxbleAmnt = 0.00M;
                            decimal spTaxableAmntOT = 0.00M;
                            decimal spTaxableAmntLWOP = 0.00M;
                            decimal DaysCnt = 0;
                            decimal payDays = 0.00M;
                            decimal leaveDays = 0.00M;
                            decimal monthDays = 0.00M;
                            decimal nonRecurringTaxable = 0.00M;
                            decimal payRatio = 1.00M;
                            decimal payRatioWithLeaves = 1.00M;
                            //**********************************
                            decimal MonthHour = 0;
                            Int32 TotalMinutes = 0;
                            Int32 PresentMinutes = 0;
                            Int32 OTMinutes = 0;
                            decimal LeaveMinutesTotal = 0;
                            decimal AllowanceTriggerValue = 18 * 60;
                            //**********************************

                            DaysCnt = ds.getDaysCnt(emp, oPayrollPeriod, out payDays, out leaveDays, out monthDays);
                            if (DaysCnt == 0)
                            {
                                MsgWarning("Zero paid days for Employee : " + emp.EmpID);
                                continue;
                            } 
                            MonthHour = Convert.ToDecimal(emp.CfgPayrollDefination.WorkHours);
                            TotalMinutes = Convert.ToInt32(monthDays * MonthHour * 60);
                            PresentMinutes = Convert.ToInt32(DaysCnt * MonthHour * 60);
                            decimal employeeRemainingSalary = 0.00M;
                            payRatio = payDays / monthDays;
                            payRatioWithLeaves = (payDays - leaveDays) / monthDays;
                            if (CompanyName != null)
                            {
                                if (CompanyName.ToLower() == "emco")
                                {
                                    if (!string.IsNullOrEmpty(emp.EmployeeContractType) && emp.EmployeeContractType == "DWGS")
                                    {
                                        empGrossSalary = ds.getEmpGross(emp, oPayrollPeriod.ID, 0, Percent);
                                        empBasicSalary = empGrossSalary;
                                    }
                                    else
                                    {
                                        empGrossSalary = ds.getEmpGross(emp, RoundingSet, 0, Percent);
                                    }
                                }
                                else
                                {
                                    empGrossSalary = ds.getEmpGross(emp, RoundingSet, 0, Percent);
                                }
                            }
                            else
                            {
                                empGrossSalary = ds.getEmpGross(emp, RoundingSet, 0, Percent);
                            }
                            prog.Text = "(" + prog.Value.ToString() + " of " + totalCnt.ToString() + " ) Processing Salary --> " + emp.FirstName + " " + emp.LastName;
                            strProcessing = "Error in Processing Salary --> " + emp.EmpID + ":" + emp.FirstName + " " + emp.LastName + " (" + i.ToString() + " of " + totalCnt.ToString() + " ) ";
                            Application.DoEvents();

                            try
                            {
                                if (!string.IsNullOrEmpty(emp.EmployeeContractType) && emp.EmployeeContractType != "DWGS")
                                {
                                    if (oPayrollPeriod.CfgPayrollDefination.PayrollType.Trim() == "HMNT")
                                    {
                                        if (emp.JoiningDate >= oPayrollPeriod.StartDate && emp.JoiningDate <= oPayrollPeriod.EndDate)
                                        {
                                            employeeRemainingSalary = empBasicSalary;
                                        }
                                        else
                                        {
                                            employeeRemainingSalary = mfmRoudingValues((decimal)empBasicSalary * payRatio, RoundingSet);
                                        }
                                    }
                                    else
                                    {
                                        employeeRemainingSalary = mfmRoudingValues((decimal)empBasicSalary * payRatio, RoundingSet);
                                    }
                                }
                                else if (CompanyName.ToLower() == "emco" && !string.IsNullOrEmpty(emp.EmployeeContractType) && emp.EmployeeContractType == "DWGS")
                                {
                                    employeeRemainingSalary = mfmRoudingValues((decimal)empBasicSalary * payRatio, RoundingSet);
                                }
                                else
                                {

                                    TrnsEmployeeWorkDays oEmpDW = (from dw in oDBPrivate.TrnsEmployeeWorkDays
                                                                   where dw.PayrollID == oPayroll.ID
                                                                   && dw.PayrollPeriodID == oPayrollPeriod.ID
                                                                   select dw).FirstOrDefault();
                                    if (oEmpDW != null)
                                    {
                                        TrnsEmployeeWDDetails oEmpDWDetails = (from a in oDBPrivate.TrnsEmployeeWDDetails
                                                                               where a.EmployeeID == emp.ID
                                                                               && a.EmpWDId == oEmpDW.Id
                                                                               select a).FirstOrDefault();
                                        if (oEmpDWDetails != null)
                                        {
                                            employeeRemainingSalary = mfmRoudingValues((decimal)oEmpDWDetails.NetIncome, RoundingSet);
                                        }
                                    }

                                }
                            }
                            catch { }
                            TrnsSalaryProcessRegister reg = new TrnsSalaryProcessRegister();
                            reg.MstEmployee = emp;
                            reg.CfgPayrollDefination = oPayroll;
                            reg.CfgPeriodDates = oPayrollPeriod;
                            reg.EmpBasic = employeeRemainingSalary;//Math.Round(Convert.ToDecimal(empBasicSalary * payRatio), 0);
                            reg.EmpGross = empGrossSalary;
                            if (emp.MstDepartment != null)
                            {
                                reg.EmpDepartment = emp.MstDepartment.DeptName;
                            }
                            else
                            {
                                reg.EmpDepartment = "";
                            }
                            if (emp.MstDesignation != null)
                            {
                                reg.EmpDesignation = emp.MstDesignation.Description;
                            }
                            else
                            {
                                reg.EmpDesignation = "";
                            }
                            if (emp.MstLocation != null)
                            {
                                reg.EmpLocation = emp.MstLocation.Description;
                            }
                            else
                            {
                                reg.EmpLocation = "";
                            }
                            if (emp.MstBranches != null)
                            {
                                reg.EmpBranch = emp.MstBranches.Name.Trim();
                            }
                            else
                            {
                                reg.EmpBranch = "";
                            }
                            if (emp.MstPosition != null)
                            {
                                reg.EmpPosition = emp.MstPosition.Description;
                            }
                            else
                            {
                                reg.EmpPosition = "";
                            }
                            if (string.IsNullOrEmpty(emp.JobTitle))
                            {
                                reg.EmpJobTitle = "";
                            }
                            else
                            {
                                MstJobTitle oTitle = (from a in oDBPrivate.MstJobTitle where a.Id.ToString() == emp.JobTitle select a).FirstOrDefault();
                                if (oTitle != null)
                                {
                                    reg.EmpJobTitle = oTitle.Description;
                                }
                            }
                            if (!string.IsNullOrEmpty(emp.CostCenter))
                            {
                                reg.EmpCostCenter = emp.CostCenter.Trim();
                            }
                            else
                            {
                                reg.EmpCostCenter = "";
                            }
                            if (!string.IsNullOrEmpty(emp.Project))
                            {
                                reg.EmpProject = emp.Project.Trim();
                            }
                            else
                            {
                                reg.EmpProject = "";
                            }
                            if (!string.IsNullOrEmpty(emp.Dimension1))
                            {
                                reg.EmpD1 = emp.Dimension1.Trim();
                            }
                            else
                            {
                                reg.EmpD1 = "";
                            }
                            if (!string.IsNullOrEmpty(emp.Dimension2))
                            {
                                reg.EmpD2 = emp.Dimension2.Trim();
                            }
                            else
                            {
                                reg.EmpD2 = "";
                            }
                            if (!string.IsNullOrEmpty(emp.Dimension3))
                            {
                                reg.EmpD3 = emp.Dimension3.Trim();
                            }
                            else
                            {
                                reg.EmpD3 = "";
                            }
                            if (!string.IsNullOrEmpty(emp.Dimension4))
                            {
                                reg.EmpD4 = emp.Dimension4.Trim();
                            }
                            else
                            {
                                reg.EmpD4 = "";
                            }
                            if (!string.IsNullOrEmpty(emp.Dimension5))
                            {
                                reg.EmpD5 = emp.Dimension5.Trim();
                            }
                            else
                            {
                                reg.EmpD5 = "";
                            }
                            reg.CreateDate = DateTime.Now;
                            reg.UpdateDate = DateTime.Now;
                            reg.UserId = oCompany.UserName;
                            reg.UpdatedBy = oCompany.UserName;
                            reg.PeriodName = oPayrollPeriod.PeriodName;
                            reg.PayrollName = oPayroll.PayrollName;
                            reg.EmpName = emp.FirstName + " " + emp.LastName;
                            //reg.DaysPaid = Convert.ToInt16( payDays);
                            reg.DaysPaid = Convert.ToDecimal(DaysCnt);
                            reg.MonthDays = Convert.ToInt32(monthDays);
                            #endregion

                            #region Employee Referrals

                            ds.EmployeeReferralsPayments(emp, oPayrollPeriod);

                            #endregion

                            #region Employee Attendance Allowance
                            Int32 CountAttendanceAlowanceMaster = (from a in oDBPrivate.MstAttendanceAllowance
                                                                   select a).Count();
                            MstElements oElement = (from a in oDBPrivate.MstElements where a.FlgAttendanceAllowance == true select a).FirstOrDefault();
                            if (CountAttendanceAlowanceMaster > 0)
                            {
                                if (oElement != null)
                                {
                                    //ds.EmployeeAttendanceAllowanceFromMaster(emp, oPayrollPeriod);
                                }
                            }
                            //int intPostedAtn = (from postedAtn in dbHrPayroll.TrnsAttendanceRegister where postedAtn.FlgPosted == true && oPayrollPeriod.ID == postedAtn.PeriodID select postedAtn.FlgPosted).Count();                            
                            int intPostedAtn = (from postedAtn in dbHrPayroll.TrnsAttendanceRegister where postedAtn.FlgPosted == true && oPayrollPeriod.ID == postedAtn.PeriodID && postedAtn.EmpID == emp.ID select postedAtn.FlgPosted).Count();
                            if (intPostedAtn == Convert.ToInt32(monthDays))
                            {
                                if (DaysCnt == monthDays)
                                {
                                    ds.EmployeeAttendanceAllowance(emp, oPayrollPeriod);
                                }
                            }

                            #endregion

                            #region Employee No Late Allowance

                            ds.EmployeeNoLateAllowance(emp, oPayrollPeriod);

                            #endregion

                            #region BasicSalary

                            /// Basic Salary ////
                            /// ************////
                            TrnsSalaryProcessRegisterDetail spdHeadRow = new TrnsSalaryProcessRegisterDetail();
                            spdHeadRow.LineType = "BS";
                            spdHeadRow.LineSubType = "Basic Salary";
                            //spdHeadRow.LineValue = Math.Round(employeeRemainingSalary, 0);
                            spdHeadRow.LineValue = mfmRoudingValues(employeeRemainingSalary, RoundingSet);
                            spdHeadRow.LineMemo = "Basic Salary ";
                            spdHeadRow.DebitAccount = glDetr.BasicSalary;
                            spdHeadRow.CreditAccount = glDetr.BSPayable;
                            spdHeadRow.DebitAccountName = Program.objHrmsUI.getAcctName(glDetr.BasicSalary);
                            spdHeadRow.CreditAccountName = Program.objHrmsUI.getAcctName(glDetr.BSPayable);
                            spdHeadRow.LineBaseEntry = emp.ID;
                            spdHeadRow.BaseValueCalculatedOn = employeeRemainingSalary;
                            spdHeadRow.BaseValue = employeeRemainingSalary;
                            spdHeadRow.BaseValueType = "FIX";
                            spdHeadRow.CreateDate = DateTime.Now;
                            spdHeadRow.UpdateDate = DateTime.Now;
                            spdHeadRow.UserId = oCompany.UserName;
                            spdHeadRow.UpdatedBy = oCompany.UserName;
                            spdHeadRow.NoOfDay = Convert.ToDecimal(DaysCnt);
                            spdHeadRow.TaxableAmount = employeeRemainingSalary;
                            spTaxbleAmnt += employeeRemainingSalary;
                            // employeeRemainingSalary += (decimal)spdHeadRow.LineValue;
                            reg.TrnsSalaryProcessRegisterDetail.Add(spdHeadRow);
                            #endregion

                            #region LeaveManagement
                            //* AbsentDeductions,Reimbursement
                            //////Absents ////
                            //**************////
                            decimal leaveCnt = 0.00M;
                            //DataTable dtAbsentDeduction = ds.salaryProcessingAbsents(emp, payrollperiod, (decimal)reg.EmpGross, out leaveCnt);

                            DataTable dtAbsentDeduction = ds.salaryProcessingAbsents(emp, oPayrollPeriod, (decimal)reg.EmpGross, out leaveCnt, glDetr);

                            foreach (DataRow dr in dtAbsentDeduction.Rows)
                            {
                                TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                                spdetail.LineType = dr["LineType"].ToString();
                                spdetail.LineSubType = dr["LineSubType"].ToString();
                                spdetail.LineValue = Convert.ToDecimal(dr["LineValue"]);
                                //spdetail.LineValue = mfmRoudingValues(Convert.ToDecimal(dr["LineValue"]), RoundingSet);
                                spdetail.LineMemo = dr["LineMemo"].ToString();
                                spdetail.DebitAccount = dr["DebitAccount"].ToString();
                                spdetail.CreditAccount = dr["CreditAccount"].ToString();
                                spdetail.DebitAccountName = dr["DebitAccountName"].ToString();
                                spdetail.CreditAccountName = dr["CreditAccountName"].ToString();
                                spdetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
                                spdetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
                                spdetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
                                spdetail.BaseValueType = dr["BaseValueType"].ToString(); ;
                                spdetail.CreateDate = DateTime.Now;
                                spdetail.UpdateDate = DateTime.Now;
                                spdetail.UserId = oCompany.UserName;
                                spdetail.UpdatedBy = oCompany.UserName;
                                spdetail.TaxableAmount = Convert.ToDecimal(dr["TaxbleAmnt"]);
                                spTaxbleAmnt += Convert.ToDecimal(dr["TaxbleAmnt"]);
                                employeeRemainingSalary += (decimal)spdetail.LineValue;
                                spTaxableAmntLWOP += Convert.ToDecimal(dr["TaxbleAmnt"]);
                                spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
                                reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
                            }

                            //DaysCnt -= leaveCnt;
                            //* End of Leave Deductions
                            #endregion

                            #region PayrollElements
                            //* Payroll elements assigned to employee ***Employee Elements ****** 
                            //*******************************************************************

                            //DataTable dtSalPrlElements = ds.salaryProcessingElements(emp, oPayrollPeriod, DaysCnt, empGrossSalary, glDetr, payRatio, payRatioWithLeaves, monthDays - leaveCnt, monthDays);
                            //foreach (DataRow dr in dtSalPrlElements.Rows)
                            //{
                            //    if (Math.Round(Convert.ToDecimal(dr["LineValue"]), 0) != 0)
                            //    {
                            //        TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                            //        spdetail.LineType = dr["LineType"].ToString();
                            //        spdetail.LineSubType = dr["LineSubType"].ToString();
                            //        spdetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                            //        spdetail.LineMemo = dr["LineMemo"].ToString();
                            //        spdetail.DebitAccount = dr["DebitAccount"].ToString();
                            //        spdetail.CreditAccount = dr["CreditAccount"].ToString();
                            //        spdetail.DebitAccountName = dr["DebitAccountName"].ToString();
                            //        spdetail.CreditAccountName = dr["CreditAccountName"].ToString();
                            //        spdetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
                            //        spdetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
                            //        spdetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
                            //        spdetail.BaseValueType = dr["BaseValueType"].ToString(); ;
                            //        spdetail.CreateDate = DateTime.Now;
                            //        spdetail.UpdateDate = DateTime.Now;
                            //        spdetail.UserId = oCompany.UserName;
                            //        spdetail.UpdatedBy = oCompany.UserName;
                            //        spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
                            //        spdetail.TaxableAmount = Convert.ToDecimal(dr["TaxbleAmnt"]);
                            //        spTaxbleAmnt += Convert.ToDecimal(dr["TaxbleAmnt"]);
                            //        nonRecurringTaxable += Convert.ToDecimal(dr["NRTaxbleAmnt"]);
                            //        employeeRemainingSalary += (decimal)spdetail.LineValue;
                            //        reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
                            //    }
                            //}
                            //******************** End of Elements *********************************
                            #endregion

                            #region PayrollElements New

                            //* Payroll elements assigned to employee ***Employee Elements ****** 
                            //*******************************************************************
                            #region Earnings
                            DataTable dtEarnings;
                            if (oPayrollPeriod.CfgPayrollDefination.PayrollType.Trim() == "HMNT")
                            {
                                if (CompanyName.ToLower() == "pakola")
                                {
                                    dtEarnings = ds.ElementsProcessionEarnings_ForHalfMonthPakola(emp, oPayrollPeriod, DaysCnt, empGrossSalary, glDetr, payRatio, payRatioWithLeaves, monthDays - leaveCnt, monthDays, RoundingSet);
                                }
                                else
                                {
                                    dtEarnings = ds.ElementsProcessionEarnings_ForHalfMonth(emp, oPayrollPeriod, DaysCnt, empGrossSalary, glDetr, payRatio, payRatioWithLeaves, monthDays - leaveCnt, monthDays, RoundingSet);
                                }
                            }
                            else
                            {
                                dtEarnings = ds.ElementsProcessionEarnings(emp, oPayrollPeriod, DaysCnt, empGrossSalary, glDetr, payRatio, payRatioWithLeaves, monthDays - leaveCnt, monthDays, RoundingSet, Percent);
                            }
                            foreach (DataRow dr in dtEarnings.Rows)
                            {
                                if (Math.Round(Convert.ToDecimal(dr["LineValue"]), 0) != 0)
                                {
                                    TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                                    spdetail.LineType = dr["LineType"].ToString();
                                    spdetail.LineSubType = dr["LineSubType"].ToString();
                                    //spdetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                                    spdetail.LineValue = mfmRoudingValues(Convert.ToDecimal(dr["LineValue"]), RoundingSet);
                                    spdetail.LineMemo = dr["LineMemo"].ToString();
                                    spdetail.DebitAccount = dr["DebitAccount"].ToString();
                                    spdetail.CreditAccount = dr["CreditAccount"].ToString();
                                    spdetail.DebitAccountName = dr["DebitAccountName"].ToString();
                                    spdetail.CreditAccountName = dr["CreditAccountName"].ToString();
                                    spdetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
                                    spdetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
                                    spdetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
                                    spdetail.BaseValueType = dr["BaseValueType"].ToString(); ;
                                    spdetail.CreateDate = DateTime.Now;
                                    spdetail.UpdateDate = DateTime.Now;
                                    spdetail.UserId = oCompany.UserName;
                                    spdetail.UpdatedBy = oCompany.UserName;
                                    spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
                                    spdetail.TaxableAmount = Convert.ToDecimal(dr["TaxbleAmnt"]);
                                    spTaxbleAmnt += Convert.ToDecimal(dr["TaxbleAmnt"]);
                                    nonRecurringTaxable += Convert.ToDecimal(dr["NRTaxbleAmnt"]);
                                    employeeRemainingSalary += (decimal)spdetail.LineValue;
                                    reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
                                }
                            }
                            #endregion

                            #region Deductions
                            DataTable dtDeductions;
                            if (oPayrollPeriod.CfgPayrollDefination.PayrollType.Trim() == "HMNT")
                            {
                                dtDeductions = null;
                                if (CompanyName.ToLower() != "pakola")
                                {
                                    dtDeductions = ds.ElementsProcessingDeductions_ForHalfMonth(emp, oPayrollPeriod, DaysCnt, empGrossSalary, glDetr, payRatio, payRatioWithLeaves, monthDays - leaveCnt, monthDays, RoundingSet);
                                }
                                else
                                {
                                    dtDeductions = ds.ElementsProcessingDeductions(emp, oPayrollPeriod, DaysCnt, empGrossSalary, glDetr, payRatio, payRatioWithLeaves, monthDays - leaveCnt, monthDays, RoundingSet);
                                }
                            }
                            else
                            {
                                dtDeductions = ds.ElementsProcessingDeductions(emp, oPayrollPeriod, DaysCnt, empGrossSalary, glDetr, payRatio, payRatioWithLeaves, monthDays - leaveCnt, monthDays, RoundingSet);
                            }

                            foreach (DataRow dr in dtDeductions.Rows)
                            {
                                if (Math.Round(Convert.ToDecimal(dr["LineValue"]), 0) != 0)
                                {
                                    TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                                    spdetail.LineType = dr["LineType"].ToString();
                                    spdetail.LineSubType = dr["LineSubType"].ToString();
                                    //spdetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                                    spdetail.LineValue = mfmRoudingValues(Convert.ToDecimal(dr["LineValue"]), RoundingSet);
                                    spdetail.LineMemo = dr["LineMemo"].ToString();
                                    spdetail.DebitAccount = dr["DebitAccount"].ToString();
                                    spdetail.CreditAccount = dr["CreditAccount"].ToString();
                                    spdetail.DebitAccountName = dr["DebitAccountName"].ToString();
                                    spdetail.CreditAccountName = dr["CreditAccountName"].ToString();
                                    spdetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
                                    spdetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
                                    spdetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
                                    spdetail.BaseValueType = dr["BaseValueType"].ToString(); ;
                                    spdetail.CreateDate = DateTime.Now;
                                    spdetail.UpdateDate = DateTime.Now;
                                    spdetail.UserId = oCompany.UserName;
                                    spdetail.UpdatedBy = oCompany.UserName;
                                    spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
                                    spdetail.TaxableAmount = Convert.ToDecimal(dr["TaxbleAmnt"]);
                                    spTaxbleAmnt += Convert.ToDecimal(dr["TaxbleAmnt"]);
                                    nonRecurringTaxable += Convert.ToDecimal(dr["NRTaxbleAmnt"]);
                                    employeeRemainingSalary += (decimal)spdetail.LineValue;
                                    reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
                                }
                            }
                            #endregion

                            if (CompanyName != null)
                            {
                                if (CompanyName.ToLower() == "spell")
                                {
                                    #region Contribution Based On Earned Salary

                                    DataTable dtContributionsBasedOnEarnedSalary = ds.ElementsContributionsCalculationBasedOnEarnedSalary(emp, oPayrollPeriod, DaysCnt, empGrossSalary, glDetr, payRatio, payRatioWithLeaves, monthDays - leaveCnt, monthDays, RoundingSet);
                                    foreach (DataRow dr in dtContributionsBasedOnEarnedSalary.Rows)
                                    {
                                        if (Math.Round(Convert.ToDecimal(dr["LineValue"]), 0) != 0)
                                        {
                                            TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                                            spdetail.LineType = dr["LineType"].ToString();
                                            spdetail.LineSubType = dr["LineSubType"].ToString();
                                            //spdetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                                            spdetail.LineValue = mfmRoudingValues(Convert.ToDecimal(dr["LineValue"]), RoundingSet);
                                            spdetail.LineMemo = dr["LineMemo"].ToString();
                                            spdetail.DebitAccount = dr["DebitAccount"].ToString();
                                            spdetail.CreditAccount = dr["CreditAccount"].ToString();
                                            spdetail.DebitAccountName = dr["DebitAccountName"].ToString();
                                            spdetail.CreditAccountName = dr["CreditAccountName"].ToString();
                                            spdetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
                                            spdetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
                                            spdetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
                                            spdetail.BaseValueType = dr["BaseValueType"].ToString(); ;
                                            spdetail.CreateDate = DateTime.Now;
                                            spdetail.UpdateDate = DateTime.Now;
                                            spdetail.UserId = oCompany.UserName;
                                            spdetail.UpdatedBy = oCompany.UserName;
                                            spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
                                            spdetail.TaxableAmount = Convert.ToDecimal(dr["TaxbleAmnt"]);
                                            spTaxbleAmnt += Convert.ToDecimal(dr["TaxbleAmnt"]);
                                            nonRecurringTaxable += Convert.ToDecimal(dr["NRTaxbleAmnt"]);
                                            employeeRemainingSalary += (decimal)spdetail.LineValue;
                                            reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
                                        }
                                    }
                                    #endregion
                                }
                                else
                                {
                                    DataTable dtContributions = ds.ElementsProcessingContributions(emp, oPayrollPeriod, DaysCnt, empGrossSalary, glDetr, payRatio, payRatioWithLeaves, monthDays - leaveCnt, monthDays, RoundingSet);
                                    foreach (DataRow dr in dtContributions.Rows)
                                    {
                                        if (Math.Round(Convert.ToDecimal(dr["LineValue"]), 0) != 0)
                                        {
                                            TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                                            spdetail.LineType = dr["LineType"].ToString();
                                            spdetail.LineSubType = dr["LineSubType"].ToString();
                                            //spdetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                                            spdetail.LineValue = mfmRoudingValues(Convert.ToDecimal(dr["LineValue"]), RoundingSet);
                                            spdetail.LineMemo = dr["LineMemo"].ToString();
                                            spdetail.DebitAccount = dr["DebitAccount"].ToString();
                                            spdetail.CreditAccount = dr["CreditAccount"].ToString();
                                            spdetail.DebitAccountName = dr["DebitAccountName"].ToString();
                                            spdetail.CreditAccountName = dr["CreditAccountName"].ToString();
                                            spdetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
                                            spdetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
                                            spdetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
                                            spdetail.BaseValueType = dr["BaseValueType"].ToString(); ;
                                            spdetail.CreateDate = DateTime.Now;
                                            spdetail.UpdateDate = DateTime.Now;
                                            spdetail.UserId = oCompany.UserName;
                                            spdetail.UpdatedBy = oCompany.UserName;
                                            spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
                                            spdetail.TaxableAmount = Convert.ToDecimal(dr["TaxbleAmnt"]);
                                            spTaxbleAmnt += Convert.ToDecimal(dr["TaxbleAmnt"]);
                                            nonRecurringTaxable += Convert.ToDecimal(dr["NRTaxbleAmnt"]);
                                            employeeRemainingSalary += (decimal)spdetail.LineValue;
                                            reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                DataTable dtContributions = ds.ElementsProcessingContributions(emp, oPayrollPeriod, DaysCnt, empGrossSalary, glDetr, payRatio, payRatioWithLeaves, monthDays - leaveCnt, monthDays, RoundingSet);
                                foreach (DataRow dr in dtContributions.Rows)
                                {
                                    if (Math.Round(Convert.ToDecimal(dr["LineValue"]), 0) != 0)
                                    {
                                        TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                                        spdetail.LineType = dr["LineType"].ToString();
                                        spdetail.LineSubType = dr["LineSubType"].ToString();
                                        //spdetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                                        spdetail.LineValue = mfmRoudingValues(Convert.ToDecimal(dr["LineValue"]), RoundingSet);
                                        spdetail.LineMemo = dr["LineMemo"].ToString();
                                        spdetail.DebitAccount = dr["DebitAccount"].ToString();
                                        spdetail.CreditAccount = dr["CreditAccount"].ToString();
                                        spdetail.DebitAccountName = dr["DebitAccountName"].ToString();
                                        spdetail.CreditAccountName = dr["CreditAccountName"].ToString();
                                        spdetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
                                        spdetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
                                        spdetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
                                        spdetail.BaseValueType = dr["BaseValueType"].ToString(); ;
                                        spdetail.CreateDate = DateTime.Now;
                                        spdetail.UpdateDate = DateTime.Now;
                                        spdetail.UserId = oCompany.UserName;
                                        spdetail.UpdatedBy = oCompany.UserName;
                                        spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
                                        spdetail.TaxableAmount = Convert.ToDecimal(dr["TaxbleAmnt"]);
                                        spTaxbleAmnt += Convert.ToDecimal(dr["TaxbleAmnt"]);
                                        nonRecurringTaxable += Convert.ToDecimal(dr["NRTaxbleAmnt"]);
                                        employeeRemainingSalary += (decimal)spdetail.LineValue;
                                        reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
                                    }
                                }
                            }
                            #endregion

                            #region OvertimeSection
                            //////Over time ////
                            //**************////

                            DataTable dtSalOverTimes = ds.salaryProcessingOvertimes(emp, oPayrollPeriod, empGrossSalary, out OTMinutes);

                            //Code modified by Zeeshan

                            foreach (DataRow dr in dtSalOverTimes.Rows)
                            {
                                TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                                spdetail.LineType = dr["LineType"].ToString();
                                spdetail.LineSubType = dr["LineSubType"].ToString();
                                //spdetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                                spdetail.LineValue = mfmRoudingValues(Convert.ToDecimal(dr["LineValue"]), RoundingSet);
                                spdetail.OTHours = Convert.ToDecimal(OTMinutes);
                                spdetail.LineMemo = dr["LineMemo"].ToString();
                                spdetail.DebitAccount = dr["DebitAccount"].ToString();
                                spdetail.CreditAccount = dr["CreditAccount"].ToString();
                                spdetail.DebitAccountName = dr["DebitAccountName"].ToString();
                                spdetail.CreditAccountName = dr["CreditAccountName"].ToString();
                                spdetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
                                spdetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
                                spdetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
                                spdetail.BaseValueType = dr["BaseValueType"].ToString(); ;
                                spdetail.CreateDate = DateTime.Now;
                                spdetail.UpdateDate = DateTime.Now;
                                spdetail.UserId = oCompany.UserName;
                                spdetail.UpdatedBy = oCompany.UserName;
                                spdetail.TaxableAmount = Convert.ToDecimal(dr["TaxbleAmnt"]);
                                spTaxbleAmnt += Convert.ToDecimal(dr["TaxbleAmnt"]);
                                spTaxableAmntOT += Convert.ToDecimal(dr["TaxbleAmnt"]);
                                employeeRemainingSalary += (decimal)spdetail.LineValue;
                                spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
                                reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
                            }
                            #endregion

                            #region Advance
                            // * ************Advance Recovery Processing **************
                            //*******************************************************
                            DataTable dtAdvance = ds.salaryProcessingAdvance(emp, employeeRemainingSalary, oPayrollPeriod);

                            foreach (DataRow dr in dtAdvance.Rows)
                            {
                                TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                                spdetail.LineType = dr["LineType"].ToString();
                                spdetail.LineSubType = dr["LineSubType"].ToString();
                                //spdetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                                spdetail.LineValue = mfmRoudingValues(Convert.ToDecimal(dr["LineValue"]), RoundingSet);
                                spdetail.LineMemo = dr["LineMemo"].ToString();
                                spdetail.DebitAccount = dr["DebitAccount"].ToString();
                                spdetail.CreditAccount = dr["CreditAccount"].ToString();
                                spdetail.DebitAccountName = dr["DebitAccountName"].ToString();
                                spdetail.CreditAccountName = dr["CreditAccountName"].ToString();
                                spdetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
                                spdetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
                                spdetail.A1Indicators = Convert.ToString(dr["Indicator"]);
                                spdetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
                                spdetail.BaseValueType = dr["BaseValueType"].ToString(); ;
                                spdetail.CreateDate = DateTime.Now;
                                spdetail.UpdateDate = DateTime.Now;
                                spdetail.UserId = oCompany.UserName;
                                spdetail.UpdatedBy = oCompany.UserName;
                                spdetail.TaxableAmount = 0.00M;
                                employeeRemainingSalary += (decimal)spdetail.LineValue;


                                spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
                                reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
                            }
                            #endregion

                            #region Loan
                            // * ************Loan Recovery Processing **************

                            DataTable dtLoands = ds.salaryProcessingLoans(emp, employeeRemainingSalary, oPayrollPeriod);

                            foreach (DataRow dr in dtLoands.Rows)
                            {
                                TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                                spdetail.LineType = dr["LineType"].ToString();
                                spdetail.LineSubType = dr["LineSubType"].ToString();
                                //spdetail.LineValue = Math.Round(Convert.ToDecimal(dr["LineValue"]), 0);
                                spdetail.LineValue = mfmRoudingValues(Convert.ToDecimal(dr["LineValue"]), RoundingSet);
                                spdetail.LineMemo = dr["LineMemo"].ToString();
                                spdetail.DebitAccount = dr["DebitAccount"].ToString();
                                spdetail.CreditAccount = dr["CreditAccount"].ToString();
                                spdetail.DebitAccountName = dr["DebitAccountName"].ToString();
                                spdetail.CreditAccountName = dr["CreditAccountName"].ToString();
                                spdetail.A1Indicators = Convert.ToString(dr["Indicator"]);
                                spdetail.LineBaseEntry = Convert.ToInt32(dr["LineBaseEntry"]);
                                spdetail.BaseValueCalculatedOn = Convert.ToDecimal(dr["BaseValueCalculatedOn"]);
                                spdetail.BaseValue = Convert.ToDecimal(dr["BaseValue"]); ;
                                spdetail.BaseValueType = dr["BaseValueType"].ToString(); ;
                                spdetail.CreateDate = DateTime.Now;
                                spdetail.UpdateDate = DateTime.Now;
                                spdetail.UserId = oCompany.UserName;
                                spdetail.UpdatedBy = oCompany.UserName;
                                spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
                                spdetail.TaxableAmount = 0.00M;
                                employeeRemainingSalary += (decimal)spdetail.LineValue;
                                reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
                            }
                            #endregion

                            #region Attendance Allowance

                            // Created for MEPL contractual staff 
                            // which can be set from position master
                            if (Convert.ToBoolean(Program.systemInfo.FlgUnitFeature) == true)
                            {
                                if (emp.MstPosition != null)
                                {
                                    if (emp.MstPosition.Name.ToLower() == "con")
                                    {
                                        decimal LateMinutes = 0;
                                        LateMinutes = TotalMinutes - PresentMinutes;
                                        //if (LateMinutes > OTMinutes)
                                        //{
                                        //    LateMinutes = LateMinutes - OTMinutes;
                                        //}
                                        //else
                                        //{
                                        //    LateMinutes = 0;
                                        //}
                                        if (LateMinutes < AllowanceTriggerValue)
                                        {
                                            decimal AttAllowanceFixValue = 300.0M;
                                            TrnsSalaryProcessRegisterDetail oDetailAttalw = new TrnsSalaryProcessRegisterDetail();
                                            oDetailAttalw.LineType = "AttAlw";
                                            oDetailAttalw.LineSubType = "Attandance Allowance";
                                            //oDetailAttalw.LineValue = Math.Round(AttAllowanceFixValue, 0);
                                            oDetailAttalw.LineValue = mfmRoudingValues(AttAllowanceFixValue, RoundingSet);
                                            oDetailAttalw.LineMemo = "Attandance Allowance";
                                            oDetailAttalw.DebitAccount = glDetr.BasicSalary;
                                            oDetailAttalw.CreditAccount = glDetr.BSPayable;
                                            oDetailAttalw.DebitAccountName = Program.objHrmsUI.getAcctName(glDetr.BasicSalary);
                                            oDetailAttalw.CreditAccountName = Program.objHrmsUI.getAcctName(glDetr.BSPayable);
                                            oDetailAttalw.LineBaseEntry = emp.ID;
                                            oDetailAttalw.BaseValueCalculatedOn = AttAllowanceFixValue;
                                            oDetailAttalw.BaseValue = AttAllowanceFixValue;
                                            oDetailAttalw.BaseValueType = "FIX";
                                            oDetailAttalw.CreateDate = DateTime.Now;
                                            oDetailAttalw.UpdateDate = DateTime.Now;
                                            oDetailAttalw.UserId = oCompany.UserName;
                                            spdHeadRow.UpdatedBy = oCompany.UserName;
                                            oDetailAttalw.NoOfDay = Convert.ToDecimal(DaysCnt);
                                            oDetailAttalw.TaxableAmount = AttAllowanceFixValue;
                                            spTaxbleAmnt += AttAllowanceFixValue;
                                            // employeeRemainingSalary += (decimal)spdHeadRow.LineValue;
                                            reg.TrnsSalaryProcessRegisterDetail.Add(oDetailAttalw);
                                        }
                                    }
                                }
                            }

                            #endregion

                            #region Taxation
                            reg.EmpTaxblTotal = spTaxbleAmnt;
                            // * ************TAX**************
                            Decimal QuaterlyTaxValueReturn = 0.0M;
                            if (Program.systemInfo.TaxSetup == true && emp.FlgTax == true)
                            {
                                //decimal TotalTax = ds.getEmployeeTaxAmount(payrollperiod, emp, spTaxableAmntOT, spTaxableAmntLWOP, spTaxbleAmnt, nonRecurringTaxable);
                                decimal TotalTax = 0;
                                if (Program.systemInfo.TaxConfiguration == 1 || Program.systemInfo.TaxConfiguration == null)
                                {
                                    TotalTax = ds.getEmployeeTaxAmountIncentivePayment(oPayrollPeriod, emp, spTaxableAmntOT, spTaxableAmntLWOP, spTaxbleAmnt, nonRecurringTaxable, empGrossSalary, payRatio, out QuaterlyTaxValueReturn);
                                }
                                else if (Program.systemInfo.TaxConfiguration == 2)
                                {
                                    TotalTax = ds.getEmployeeTaxAmountEgytianLaw(oPayrollPeriod, emp, spTaxableAmntOT, spTaxableAmntLWOP, spTaxbleAmnt, nonRecurringTaxable, empGrossSalary, payRatio, out QuaterlyTaxValueReturn);
                                }
                                if (TotalTax >= 0)
                                {
                                    reg.EmpTotalTax = TotalTax;

                                    TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                                    spdetail.LineType = "Tax";
                                    spdetail.LineSubType = "Tax";
                                    //spdetail.LineValue = -Math.Round(TotalTax, 0);
                                    spdetail.LineValue = -mfmRoudingValues(TotalTax, RoundingSet);
                                    spdetail.LineMemo = "Tax Deduction";
                                    spdetail.DebitAccount = glDetr.IncomeTaxExpense;
                                    spdetail.CreditAccount = glDetr.IncomeTaxPayable;
                                    spdetail.DebitAccountName = Program.objHrmsUI.getAcctName(glDetr.IncomeTaxExpense);
                                    spdetail.CreditAccountName = Program.objHrmsUI.getAcctName(glDetr.IncomeTaxPayable);
                                    spdetail.LineBaseEntry = 0;
                                    spdetail.BaseValueCalculatedOn = spTaxbleAmnt + QuaterlyTaxValueReturn;
                                    spdetail.BaseValue = spTaxbleAmnt + QuaterlyTaxValueReturn;
                                    spdetail.BaseValueType = "FIX";
                                    spdetail.CreateDate = DateTime.Now;
                                    spdetail.UpdateDate = DateTime.Now;
                                    spdetail.UserId = oCompany.UserName;
                                    spdetail.UpdatedBy = oCompany.UserName;
                                    spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
                                    spdetail.TaxableAmount = 0.00M;
                                    reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
                                }
                            }
                            spTaxbleAmnt = spTaxbleAmnt + QuaterlyTaxValueReturn;
                            //reg.EmpTaxblTotal = spTaxbleAmnt;
                            #endregion

                            #region Gratuity
                            //************************************************
                            //********** Gratuity Calculations ***************
                            if (Program.systemInfo.FlgArabic == true)
                            {
                                DataTable dtGratuity = ds.GratuityCalculations(emp, monthDays, oPayrollPeriod);
                                //CfgPeriodDates oPrePeriod = GetPreviousPeriod(oPayroll, oPayrollPeriod);
                                //DataTable dtGratuity = ds.GratuitySlabWiseCalculations(emp, monthDays, oPayrollPeriod, oPrePeriod);
                                foreach (DataRow dr in dtGratuity.Rows)
                                {
                                    if (Math.Round(Convert.ToDecimal(dr["LineValue"]), 0) != 0)
                                    {
                                        TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                                        spdetail.LineType = dr["LineType"].ToString();
                                        spdetail.LineSubType = dr["LineSubType"].ToString();
                                        spdetail.LineValue = mfmRoudingValues(Convert.ToDecimal(dr["LineValue"]), RoundingSet);
                                        spdetail.LineMemo = dr["LineMemo"].ToString();
                                        spdetail.DebitAccount = glDetr.GratuityExpense;
                                        spdetail.CreditAccount = glDetr.GratuityPayable;
                                        spdetail.DebitAccountName = Program.objHrmsUI.getAcctName(glDetr.GratuityExpense);
                                        spdetail.CreditAccountName = Program.objHrmsUI.getAcctName(glDetr.GratuityPayable);
                                        spdetail.LineBaseEntry = 0;
                                        spdetail.BaseValueCalculatedOn = mfmRoudingValues(Convert.ToDecimal(dr["BaseValueCalculatedOn"]), RoundingSet);
                                        spdetail.BaseValue = mfmRoudingValues(Convert.ToDecimal(dr["BaseValue"]), RoundingSet);
                                        spdetail.BaseValueType = "FIX";
                                        spdetail.CreateDate = DateTime.Now;
                                        spdetail.UpdateDate = DateTime.Now;
                                        spdetail.UserId = oCompany.UserName;
                                        spdetail.UpdatedBy = oCompany.UserName;
                                        spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
                                        spdetail.TaxableAmount = 0.00M;
                                        reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
                                    }
                                }
                            }
                            //if (emp.CfgPayrollDefination.FlgGratuity == true)
                            //int gratCnt = (from p in oDBPrivate.MstGratuity where p.Id == emp.CfgPayrollDefination.GratuityID select p).Count();
                            //if (gratCnt > 0)
                            //{
                            //    MstGratuity empGrat = (from p in oDBPrivate.MstGratuity where p.Id == emp.CfgPayrollDefination.GratuityID select p).FirstOrDefault();

                            //    try
                            //    {
                            //        int FromYr = Convert.ToInt16(empGrat.YearFrom) * 365;

                            //        if ((Convert.ToDateTime(oPayrollPeriod.StartDate) - Convert.ToDateTime(emp.JoiningDate)).Days > FromYr)
                            //        {
                            //            decimal gratProvision = 0.00M;
                            //            decimal basedOnAmont = 0.00M;
                            //            if (empGrat.BasedOn == "0")
                            //            {
                            //                basedOnAmont = empBasicSalary;
                            //            }
                            //            else
                            //            {
                            //                basedOnAmont = empGrossSalary;
                            //            }

                            //            gratProvision = (basedOnAmont * (decimal)empGrat.Factor / 100) / 12;
                            //            TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                            //            spdetail.LineType = "Element";
                            //            spdetail.LineSubType = "Empr Cont";
                            //            spdetail.LineValue = mfmRoudingValues(gratProvision, RoundingSet);
                            //            spdetail.LineMemo = "Gratuity";
                            //            spdetail.DebitAccount = glDetr.GratuityExpense;
                            //            spdetail.CreditAccount = glDetr.GratuityPayable;
                            //            spdetail.DebitAccountName = Program.objHrmsUI.getAcctName(glDetr.GratuityExpense);
                            //            spdetail.CreditAccountName = Program.objHrmsUI.getAcctName(glDetr.GratuityPayable);
                            //            spdetail.LineBaseEntry = 0;
                            //            spdetail.BaseValueCalculatedOn = empBasicSalary;
                            //            spdetail.BaseValue = empBasicSalary;
                            //            spdetail.BaseValueType = "FIX";
                            //            spdetail.CreateDate = DateTime.Now;
                            //            spdetail.UpdateDate = DateTime.Now;
                            //            spdetail.UserId = oCompany.UserName;
                            //            spdetail.UpdatedBy = oCompany.UserName;
                            //            spdetail.NoOfDay = Convert.ToDecimal(DaysCnt);
                            //            spdetail.TaxableAmount = 0.00M;
                            //            reg.TrnsSalaryProcessRegisterDetail.Add(spdetail);
                            //        }
                            //    }
                            //    catch (Exception ex)
                            //    {

                            //    }
                            //}

                            #endregion

                            reg.SalaryStatus = 0;//Salary Processed
                            if (emp.PaymentMode == "HOLD")
                            {
                                reg.FlgHoldPayment = true;
                            }
                            oDBPrivate.TrnsSalaryProcessRegister.InsertOnSubmit(reg);
                            i++;
                        }
                        oDBPrivate.SubmitChanges();
                    }
                    catch (Exception ex)
                    {
                        oApplication.SetStatusBarMessage(strProcessing + ":" + ex.Message);
                    }
                    #endregion
                    prog.Stop();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(prog);

                    prog = null;
                }
                getNPEmployees();
                getPEmployees();
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage(ex.Message);
            }
        }

        private void postBranches()
        {
            int confirm = oApplication.MessageBox("JE posting is irr-reversable. Are you sure you want to post salary? ", 3, "Yes", "No", "Cancel");
            if (confirm == 2 || confirm == 3) return;
            CfgPeriodDates payrollperiod = (from p in dbHrPayroll.CfgPeriodDates where p.ID.ToString() == cbPeriod.Value.ToString().Trim() select p).FirstOrDefault();

            int totalCnt = 0;
            string spIds = "0";
            grdEmpPost.FlushToDataSource();
            for (int i = 0; i < dtEmpsPost.Rows.Count; i++)
            {
                string sel = dtEmpsPost.GetValue("isSel", i);
                if (sel == "Y")
                {
                    if (totalCnt == 0)
                    {
                        spIds = dtEmpsPost.GetValue("id", i);
                    }
                    else
                    {
                        spIds += ", " + dtEmpsPost.GetValue("id", i);
                    }
                    totalCnt++;

                }
            }
            if (spIds == "0")
            {
                oApplication.SetStatusBarMessage("Select employees to post ");
            }
            SearchKeyVal.Clear();
            SearchKeyVal.Add("spIds", spIds);
            string JeSql = sqlString.getSql("JEBranches", SearchKeyVal);

            if (totalCnt > 0)
            {
                DataTable dtJeDetail = ds.getDataTable(JeSql);

                string errMsg = "";
                string strCode = "";
                string strName = "";
                foreach (DataRow dr in dtJeDetail.Rows)
                {
                    strCode = dr["AcctCode"].ToString();
                    strName = dr["AcctName"].ToString();
                    if (strCode == "Not Found")
                    {
                        errMsg = "GL Missing. Please confirm that GL Determination complete.";
                    }

                }
                if (errMsg != "")
                {
                    oApplication.SetStatusBarMessage(errMsg);
                    return;
                }
                TrnsJE je = new TrnsJE();
                je.CreateDt = DateTime.Now;
                je.FlgCanceled = false;
                je.FlgPosted = false;
                je.JEPostingDate = payrollperiod.EndDate;
                je.PayrollID = payrollperiod.CfgPayrollDefination.ID;
                je.PeriodID = payrollperiod.ID;
                je.Memo = " Payroll JE for period " + payrollperiod.PeriodName;

                foreach (DataRow dr in dtJeDetail.Rows)
                {
                    TrnsJEDetail jed = new TrnsJEDetail();
                    jed.AcctCode = dr["AcctCode"].ToString();
                    jed.AcctName = dr["AcctName"].ToString();
                    //jed.Debit = Convert.ToDecimal(dr["Debit"].ToString());
                    //jed.Credit = Convert.ToDecimal(dr["Credit"].ToString());
                    jed.Debit = mfmRoudingValues(Convert.ToDecimal(dr["Debit"].ToString()), RoundingSet);
                    jed.Credit = mfmRoudingValues(Convert.ToDecimal(dr["Credit"].ToString()), RoundingSet);
                    jed.CostCenter = dr["CostCenter"].ToString();
                    jed.BranchName = dr["BranchName"].ToString();
                    jed.FCurrency = dr["EmpCurr"].ToString();
                    je.TrnsJEDetail.Add(jed);
                }
                dbHrPayroll.TrnsJE.InsertOnSubmit(je);
                dbHrPayroll.SubmitChanges();
                int jeNum = je.ID;

                for (int i = 0; i < dtEmpsPost.Rows.Count; i++)
                {
                    string sel = dtEmpsPost.GetValue("isSel", i);
                    string id = dtEmpsPost.GetValue("id", i);
                    if (sel == "Y")
                    {
                        string processId = dtEmpsPost.GetValue("id", i);
                        TrnsSalaryProcessRegister sp = (from p in dbHrPayroll.TrnsSalaryProcessRegister where p.Id.ToString() == processId select p).Single();
                        sp.JENum = jeNum;
                        sp.SalaryStatus = 1;

                    }
                }
                dbHrPayroll.SubmitChanges();

                getPEmployees();
            }
        }

        private void fillJeDetail(string jeNum)
        {
            string strSql = @"SELECT     AcctCode, AcctName, Debit, Credit, JEID,CostCenter
                                FROM         dbo.trnsJEDetail where JEID='" + jeNum + @"'";
            decimal debSum = 0.0M;
            decimal credSum = 0.0M;
            DataTable dtJeDetail = ds.getDataTable(strSql);
            dtJeDet.Rows.Clear();
            int i = 0;
            foreach (DataRow dr in dtJeDetail.Rows)
            {


                dtJeDet.Rows.Add(1);
                dtJeDet.SetValue("acctCode", i, dr["AcctCode"].ToString() + "-" + dr["CostCenter"].ToString());
                dtJeDet.SetValue("acctName", i, dr["AcctName"].ToString());
                dtJeDet.SetValue("debit", i, dr["Debit"].ToString());
                dtJeDet.SetValue("credit", i, dr["Credit"].ToString());
                debSum += Convert.ToDecimal(dr["Debit"]);
                credSum += Convert.ToDecimal(dr["Credit"]);
                i++;

            }
            grdJeDet.LoadFromDataSource();
            txTotDeb.Value = debSum.ToString();
            txTotCred.Value = credSum.ToString();

        }

        private void fillJeDetailLocation(string jeNum)
        {
            string strSql = @"SELECT     AcctCode, AcctName, Debit, Credit, JEID,CostCenter,LocationID
                                FROM         dbo.trnsJEDetail where JEID='" + jeNum + @"'";
            decimal debSum = 0.0M;
            decimal credSum = 0.0M;
            DataTable dtJeDetail = ds.getDataTable(strSql);
            dtJeDet.Rows.Clear();
            int i = 0;
            foreach (DataRow dr in dtJeDetail.Rows)
            {
                var oLocation = (from a in dbHrPayroll.MstLocation where a.Id.ToString() == dr["LocationID"].ToString() select a).FirstOrDefault();

                dtJeDet.Rows.Add(1);
                dtJeDet.SetValue("acctCode", i, dr["AcctCode"].ToString());
                dtJeDet.SetValue("acctName", i, dr["AcctName"].ToString());
                dtJeDet.SetValue("debit", i, dr["Debit"].ToString());
                dtJeDet.SetValue("credit", i, dr["Credit"].ToString());
                debSum += Convert.ToDecimal(dr["Debit"]);
                credSum += Convert.ToDecimal(dr["Credit"]);
                i++;

            }
            grdJeDet.LoadFromDataSource();
            txTotDeb.Value = debSum.ToString();
            txTotCred.Value = credSum.ToString();

        }

        private void fillSalaryDetails(string salaryId)
        {
            iniSalaryDetail();
            dtPrEle.Rows.Clear();
            dtPrOth.Rows.Clear();
            decimal EleTotal = 0.0M;
            decimal OtherTotal = 0.00M;
            decimal BasicSalary = 0.00M;
            decimal GrossSalary = 0.0M;
            int i = 0;
            int cnt = (from p in dbHrPayroll.TrnsSalaryProcessRegister where p.Id.ToString() == salaryId select p).Count();
            if (cnt > 0)
            {
                TrnsSalaryProcessRegister salms = (from p in dbHrPayroll.TrnsSalaryProcessRegister where p.Id.ToString() == salaryId select p).Single();
                BasicSalary = Convert.ToDecimal(salms.EmpBasic.ToString());
                GrossSalary = Convert.ToDecimal(salms.EmpGross.ToString());
                string strSql = " SELECT     LineMemo, SUM(LineValue) AS Amnt FROM   dbo.TrnsSalaryProcessRegisterDetail WHERE (LineType = 'Element' and LineSubType <>'Empr Cont' AND LineSubType <> 'Gratuity Provision') and SRID='" + salaryId + "' GROUP BY LineMemo ";
                DataTable dtEle = ds.getDataTable(strSql);
                string strAmount = "";
                foreach (DataRow dr in dtEle.Rows)
                {
                    strAmount = "";
                    dtPrEle.Rows.Add(1);
                    dtPrEle.SetValue("Descr", i, dr["LineMemo"].ToString());
                    //strAmount = Convert.ToString(mfmRoudingValues(Convert.ToDecimal(dr["Amnt"].ToString()), RoundingSet));
                    //dtPrEle.SetValue("Amount", i, strAmount.ToString());
                    dtPrEle.SetValue("Amount", i, dr["Amnt"].ToString());

                    EleTotal += Convert.ToDecimal(dr["Amnt"].ToString());
                    // EleTotal += mfmRoudingValues(Convert.ToDecimal(dr["Amnt"].ToString()), RoundingSet);
                    i++;
                }

                i = 0;
                strSql = " SELECT     LineMemo, SUM(LineValue) AS Amnt FROM   dbo.TrnsSalaryProcessRegisterDetail WHERE (LineType <> 'Element' and LineType <>'BS') and SRID='" + salaryId + "' GROUP BY LineMemo ";
                dtEle = ds.getDataTable(strSql);
                foreach (DataRow dr in dtEle.Rows)
                {
                    strAmount = "";
                    dtPrOth.Rows.Add(1);
                    dtPrOth.SetValue("Descr", i, dr["LineMemo"].ToString());
                    strAmount = Convert.ToString(mfmRoudingValues(Convert.ToDecimal(dr["Amnt"].ToString()), RoundingSet));
                    dtPrOth.SetValue("Amount", i, strAmount.ToString());
                    //dtPrOth.SetValue("Amount", i, dr["Amnt"].ToString());
                    //OtherTotal += Convert.ToDecimal(dr["Amnt"].ToString());
                    OtherTotal += mfmRoudingValues(Convert.ToDecimal(dr["Amnt"].ToString()), RoundingSet);
                    i++;
                }
                txBasic.Value = BasicSalary.ToString();
                txEleTot.Value = EleTotal.ToString();
                txOtTot.Value = OtherTotal.ToString();
                var EmpRecord = dbHrPayroll.MstEmployee.Where(e => e.ID == salms.EmpID).FirstOrDefault();
                if (EmpRecord != null)
                {
                    CfgPayrollDefination payroll = (from p in dbHrPayroll.CfgPayrollDefination where p.ID.ToString() == cbPayroll.Value.ToString() select p).Single();
                    CfgPeriodDates payrollperiod = (from p in dbHrPayroll.CfgPeriodDates where p.ID.ToString() == cbPeriod.Value.ToString() select p).Single();
                    var ProcessedDays = dbHrPayroll.TrnsEmployeeWorkDays.Where(w => w.PayrollPeriodID == payrollperiod.ID).FirstOrDefault();
                    if (ProcessedDays != null)
                    {
                        var Days = ProcessedDays.TrnsEmployeeWDDetails.Where(wd => wd.EmployeeID == EmpRecord.ID).FirstOrDefault();
                        if (Days != null)
                        {
                            GrossSalary = (decimal)(EmpRecord.BasicSalary * Days.WorkDays);
                        }
                    }
                    if (!string.IsNullOrEmpty(EmpRecord.EmployeeContractType) && EmpRecord.EmployeeContractType == "DWGS")
                    {
                        if (CompanyName.ToLower() != "emco")
                        {
                            txNet.Value = Convert.ToString(EleTotal + OtherTotal + BasicSalary);
                        }
                        else
                        {
                            txNet.Value = Convert.ToString(EleTotal + OtherTotal + BasicSalary);
                        }
                    }
                    else
                    {
                        txNet.Value = Convert.ToString(EleTotal + OtherTotal + BasicSalary);
                    }
                }
                grdOthElem.LoadFromDataSource();
                grdElem.LoadFromDataSource();
            }
        }

        private void iniSalaryDetail()
        {
            txBasic.Value = "0.00";
            txEleTot.Value = "0.00";
            txOtTot.Value = "0.00";
            txNet.Value = "0.00";
        }

        private void VoidSalary()
        {
            try
            {
                grdEmpPost.FlushToDataSource();
                for (int i = 0; i < dtEmpsPost.Rows.Count; i++)
                {
                    string sel = dtEmpsPost.GetValue("isSel", i);
                    string status = dtEmpsPost.GetValue("Status", i);
                    if (sel == "Y" && status == "N")
                    {
                        string empid = Convert.ToString(dtEmpsPost.GetValue("empId", i));
                        string salId = dtEmpsPost.GetValue("id", i);
                        TrnsSalaryProcessRegister reg = (from p in dbHrPayroll.TrnsSalaryProcessRegister where p.Id.ToString() == salId && p.PayrollID.ToString() == cbPayroll.Value.ToString() && p.PayrollPeriodID.ToString() == cbPeriod.Value.ToString() select p).FirstOrDefault();
                        IEnumerable<TrnsEmployeeElementDetail> nonRecuringElements = from p in dbHrPayroll.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.MstEmployee.EmpID == empid && p.PeriodId.ToString() == cbPeriod.Value.ToString() select p;
                        foreach (TrnsEmployeeElementDetail ele in nonRecuringElements)
                        {
                            ele.FlgOneTimeConsumed = false;
                        }
                        if (reg != null)
                        {
                            dbHrPayroll.TrnsSalaryProcessRegister.DeleteOnSubmit(reg);
                        }
                        //dbHrPayroll.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, nonRecuringElements);                   
                    }
                }
                dbHrPayroll.SubmitChanges();

                getNPEmployees();
                getPEmployees();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Void Salary : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void printSheet()
        {
            try
            {
                string cri = " Where TrnsSalaryProcessRegister.PayrollID = '" + cbPayroll.Value.Trim() + "' and TrnsSalaryProcessRegister.PayrollPeriodID='" + cbPeriod.Value.Trim() + "' ";
                if (cbDept.Value.Trim() != "0")
                {
                    cri += " and MstDepartment.ID ='" + cbDept.Value.Trim() + "'";
                }
                if (cbLoc.Value.Trim() != "0")
                {
                    cri += " and MstLocation.Id ='" + cbLoc.Value.Trim() + "'";
                }
                Program.objHrmsUI.printRpt("Sheet", true, cri, "");
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error in showing report. " + ex.Message);
            }
        }

        private void TaxDetailInfo()
        {
            int totalCnt = 0;
            string spIds = "0";
            grdEmpPost.FlushToDataSource();
            for (int i = 0; i < dtEmpsPost.Rows.Count; i++)
            {
                string sel = dtEmpsPost.GetValue("isSel", i);
                if (sel == "Y")
                {
                    if (totalCnt == 0)
                    {
                        spIds = dtEmpsPost.GetValue("empId", i);
                    }
                    else
                    {
                        spIds += ", " + dtEmpsPost.GetValue("empId", i);
                    }
                    totalCnt++;

                }
            }
            if (spIds == "0")
            {
                oApplication.SetStatusBarMessage("Select employees to print tax detail ");
            }
            else
            {
                string cri = " WHERE dbo.LogTaxDetails.EmpID IN ('" + spIds + "') AND dbo.CfgPeriodDates.ID = '" + cbPeriod.Value.Trim() + "' and dbo.LogTaxDetails.LogType In ('Salary') ";
                Program.objHrmsUI.printRpt("TaxDetail", true, cri, "");
            }
        }

        private void printDepartSheet()
        {
            try
            {
                //string cri = " Where TrnsSalaryProcessRegister.PayrollID = '" + cbPayroll.Value.Trim() + "' and TrnsSalaryProcessRegister.PayrollPeriodID='" + cbPeriod.Value.Trim() + "' ";
                string cri = " Where DeptName IS NOT NULL AND TrnsSalaryProcessRegister.PayrollPeriodID='" + cbPeriod.Value.Trim() + "' ";
                if (cbDept.Value.Trim() != "0")
                {
                    cri += " and MstDepartment.ID ='" + cbDept.Value.Trim() + "'";
                }
                //if (cbLoc.Value.Trim() != "0")
                //{
                //    cri += " and MstLocation.Id ='" + cbLoc.Value.Trim() + "'";
                //}
                Program.objHrmsUI.printRpt("dptSheet", true, cri, "");
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error in showing report. " + ex.Message);
            }
        }

        private void printPmt()
        {
            try
            {
                string cri = " Where TrnsSalaryProcessRegister.PayrollID = '" + cbPayroll.Value.Trim() + "' and TrnsSalaryProcessRegister.PayrollPeriodID='" + cbPeriod.Value.Trim() + "' ";

                Program.objHrmsUI.printRpt("Payment", true, cri, "");
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error in showing report. " + ex.Message);
            }
        }

        private void printSlip()
        {
            int totalCnt = 0;
            string spIds = "0";
            grdEmpPost.FlushToDataSource();
            for (int i = 0; i < dtEmpsPost.Rows.Count; i++)
            {
                string sel = dtEmpsPost.GetValue("isSel", i);
                if (sel == "Y")
                {
                    if (totalCnt == 0)
                    {
                        spIds = dtEmpsPost.GetValue("id", i);
                    }
                    else
                    {
                        spIds += ", " + dtEmpsPost.GetValue("id", i);
                    }
                    totalCnt++;

                }
            }
            if (spIds == "0")
            {
                oApplication.SetStatusBarMessage("Select employees to print slip ");
            }
            else
            {
                string cri = " Where TrnsSalaryProcessRegister.Id in (" + spIds + ")";
                Program.objHrmsUI.printRpt("slip", true, cri, "");


            }
        }

        private void emailSlip()
        {
            try
            {
                int totalCnt = 0;
                string spIds = "0";
                grdEmpPost.FlushToDataSource();
                int validForEmailEmployees = 0;
                int NonFlgEmail = 0;
                int selected = 0;
                var oEmailConfig = (from a in dbHrPayroll.MstEmailConfig select a).FirstOrDefault();
                if (oEmailConfig == null)
                {
                    oApplication.StatusBar.SetText("Email Configuration didn't provided.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return;
                }
                for (int i = 0; i < dtEmpsPost.Rows.Count; i++)
                {
                    string sel = dtEmpsPost.GetValue("isSel", i);
                    if (sel.Trim().ToUpper() == "Y")
                    {
                        if (totalCnt == 0)
                        {
                            string oID = dtEmpsPost.GetValue("id", i);
                            int id = int.Parse(oID);
                            var salReg = dbHrPayroll.TrnsSalaryProcessRegister.Where(o => o.Id == id).FirstOrDefault();
                            if (salReg != null)
                            {
                                selected++;
                                if (salReg.MstEmployee.FlgEmail == true && salReg.MstEmployee.OfficeEmail != "")
                                {
                                    if (salReg.JENum != null)
                                    {
                                        var hasJEEntry = dbHrPayroll.TrnsJE.Where(o => o.ID == salReg.JENum).FirstOrDefault();
                                        if (hasJEEntry != null)
                                        {
                                            if (totalCnt == 0)
                                            {
                                                spIds = dtEmpsPost.GetValue("id", i);
                                                validForEmailEmployees++;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    NonFlgEmail++;
                                }
                            }
                        }
                        else
                        {
                            //int id = dtEmpsPost.GetValue("id", i);
                            string oID = dtEmpsPost.GetValue("id", i);
                            int id = int.Parse(oID);
                            var salReg = dbHrPayroll.TrnsSalaryProcessRegister.Where(o => o.Id == id).FirstOrDefault();
                            if (salReg != null)
                            {
                                selected++;
                                if (salReg.MstEmployee.FlgEmail == true && salReg.MstEmployee.OfficeEmail != "")
                                {
                                    if (salReg.JENum != null)
                                    {
                                        var hasJEEntry = dbHrPayroll.TrnsJE.Where(o => o.ID == salReg.JENum).FirstOrDefault();
                                        if (hasJEEntry != null)
                                        {
                                            spIds += ", " + dtEmpsPost.GetValue("id", i);
                                            validForEmailEmployees++;
                                        }
                                    }
                                }
                                else
                                {
                                    NonFlgEmail++;
                                }
                            }
                        }
                        totalCnt++;
                    }
                }
                if (spIds == "0" && selected == NonFlgEmail)
                {
                    oApplication.SetStatusBarMessage("Employees are not Subscribed for Email ");
                }
                else if (spIds == "0")
                {
                    oApplication.SetStatusBarMessage("Select Employees to Email ");
                }
                else
                {
                    if (selected > NonFlgEmail)
                    {
                        oApplication.StatusBar.SetText((selected - NonFlgEmail) + " of " + selected + " Employees will be Emailed ", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    }
                    string cri = " Where TrnsSalaryProcessRegister.Id in (" + spIds + ")";
                    Program.objHrmsUI.emailRpt("slip", true, spIds);
                }
            }
            catch (Exception ex)
            {
                Program.objHrmsUI.logger.LogException(Program.objHrmsUI.AppVersion, ex);
            }
        }

        private void reSendEmailSlip()
        {
            int totalCnt = 0;
            string spIds = "0";
            grdEmpPost.FlushToDataSource();
            for (int i = 0; i < dtEmpsPost.Rows.Count; i++)
            {
                string sel = dtEmpsPost.GetValue("isSel", i);
                if (sel == "Y")
                {
                    if (totalCnt == 0)
                    {
                        // chekc if 
                        string oID = dtEmpsPost.GetValue("id", i);
                        int id = int.Parse(oID);
                        var salReg = dbHrPayroll.TrnsSalaryProcessRegister.Where(o => o.Id == id).SingleOrDefault();
                        if (salReg != null)
                        {

                            if (salReg.MstEmployee.FlgEmail == true && salReg.MstEmployee.PersonalEmail != "")
                            {
                                if (salReg.JENum != null)
                                {
                                    var hasJEEntry = dbHrPayroll.TrnsJE.Where(o => o.ID == salReg.JENum && o.SBOJeNum != null).FirstOrDefault();
                                    if (hasJEEntry != null)
                                    {
                                        if (totalCnt == 0)
                                        {
                                            spIds = dtEmpsPost.GetValue("id", i);
                                        }
                                    }
                                }

                            }
                        }


                    }
                    else
                    {


                        //int id = dtEmpsPost.GetValue("id", i);
                        string oID = dtEmpsPost.GetValue("id", i);
                        int id = int.Parse(oID);
                        var salReg = dbHrPayroll.TrnsSalaryProcessRegister.Where(o => o.Id == id).SingleOrDefault();
                        if (salReg != null)
                        {

                            if (salReg.MstEmployee.FlgEmail == true && salReg.MstEmployee.PersonalEmail != "")
                            {
                                if (salReg.JENum != null)
                                {
                                    var hasJEEntry = dbHrPayroll.TrnsJE.Where(o => o.ID == salReg.JENum && o.SBOJeNum != null).FirstOrDefault();
                                    if (hasJEEntry != null)
                                    {
                                        spIds += ", " + dtEmpsPost.GetValue("id", i);

                                    }
                                }

                            }
                        }
                    }
                    totalCnt++;

                }
            }
            if (spIds == "0")
            {
                oApplication.SetStatusBarMessage("Select Employees to Email ");
            }
            else
            {
                string cri = " Where TrnsSalaryProcessRegister.Id in (" + spIds + ")";
                //Program.objHrmsUI.printRpt("slip", true, spIds, "");

                Program.objHrmsUI.emailRpt("slip", true, spIds);



            }
        }

        private void doPostingTransactions(int jeNum)
        {
            IEnumerable<TrnsSalaryProcessRegisterDetail> recovereddetail = from p in dbHrPayroll.TrnsSalaryProcessRegisterDetail
                                                                           where p.LineType == "Advance Recovery"
                                                                               && p.TrnsSalaryProcessRegister.CfgPeriodDates.ID.ToString() == cbPeriod.Value.ToString().Trim()
                                                                          && p.TrnsSalaryProcessRegister.JENum.ToString() == jeNum.ToString()
                                                                          && p.TrnsSalaryProcessRegister.SalaryStatus == 1

                                                                           select p;
            foreach (TrnsSalaryProcessRegisterDetail adv in recovereddetail)
            {
                TrnsAdvance empAdv = (from p in dbHrPayroll.TrnsAdvance where p.ID.ToString() == adv.LineBaseEntry.ToString() select p).Single();
                empAdv.RemainingAmount += adv.LineValue;


            }

            recovereddetail = from p in dbHrPayroll.TrnsSalaryProcessRegisterDetail
                              where p.LineType == "Loan Recovery"
                                  && p.TrnsSalaryProcessRegister.CfgPeriodDates.ID.ToString() == cbPeriod.Value.ToString().Trim()
                             && p.TrnsSalaryProcessRegister.JENum.ToString() == jeNum.ToString()
                             && p.TrnsSalaryProcessRegister.SalaryStatus == 1

                              select p;
            foreach (TrnsSalaryProcessRegisterDetail loan in recovereddetail)
            {
                TrnsLoan empLoan = (from p in dbHrPayroll.TrnsLoan where p.ID.ToString() == loan.LineBaseEntry.ToString() select p).Single();
                empLoan.TrnsLoanDetail[0].RecoveredAmount -= loan.LineValue;
            }


            dbHrPayroll.SubmitChanges();
        }

        private void AfterPostingTransactionAdvances(int JeNum)
        {
            try
            {
                var oSalaryDetailAdvances = (from a in dbHrPayroll.TrnsSalaryProcessRegisterDetail
                                             where a.LineType == "Advance Recovery"
                                             && a.TrnsSalaryProcessRegister.SalaryStatus == 1
                                             && a.TrnsSalaryProcessRegister.JENum == JeNum
                                             select a).ToList();
                foreach (var oSingleAdvance in oSalaryDetailAdvances)
                {
                    var oAdvance = (from a in dbHrPayroll.TrnsAdvance
                                    where a.ID == oSingleAdvance.LineBaseEntry
                                    select a).FirstOrDefault();
                    if (oAdvance != null)
                    {
                        decimal advrecamt = ds.GetAdvanceRecoveredAmount(Convert.ToInt32(oAdvance.ID), Convert.ToInt32(oAdvance.EmpID), Convert.ToInt32(oSingleAdvance.TrnsSalaryProcessRegister.PayrollPeriodID));
                        if (advrecamt != 0)
                        {
                            oAdvance.RemainingAmount = Convert.ToDecimal(oAdvance.ApprovedAmount) + advrecamt;
                        }
                    }
                }
                dbHrPayroll.SubmitChanges();
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private void AfterPostingTransactionLoans(int JeNum)
        {
            try
            {
                var oSalaryDetailLoans = (from a in dbHrPayroll.TrnsSalaryProcessRegisterDetail
                                          where a.LineType == "Loan Recovery"
                                          && a.TrnsSalaryProcessRegister.SalaryStatus == 1
                                          && a.TrnsSalaryProcessRegister.JENum == JeNum
                                          select a).ToList();
                foreach (var oSingleLoan in oSalaryDetailLoans)
                {
                    var oLoan = (from a in dbHrPayroll.TrnsLoanDetail
                                 //where a.LnAID == oSingleLoan.LineBaseEntry
                                 where a.ID == oSingleLoan.LineBaseEntry
                                 select a).FirstOrDefault();
                    if (oLoan != null)
                    {
                        decimal loanrecamt = ds.GetLoanRecoveredAmount(Convert.ToInt32(oLoan.ID), Convert.ToInt32(oLoan.TrnsLoan.EmpID), Convert.ToInt32(oSingleLoan.TrnsSalaryProcessRegister.PayrollPeriodID));
                        if (loanrecamt != 0)
                        {
                            oLoan.RecoveredAmount = loanrecamt;
                        }
                    }
                }
                dbHrPayroll.SubmitChanges();
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private void ProcessingChecks()
        {
            Int32 PayrollId, PeriodId, Counter;
            int LastPeriodId = 0;
            Boolean flgLastPostedPeriod = false;
            IEnumerable<CfgPeriodDates> oCollection = null;
            try
            {
                PayrollId = Convert.ToInt32(cbPayroll.Value.Trim());
                PeriodId = Convert.ToInt32(cbPeriod.Value.Trim());
                oCollection = from a in dbHrPayroll.CfgPeriodDates
                              where a.PayrollId == PayrollId
                              select a;
                Counter = 0;
                foreach (CfgPeriodDates One in oCollection)
                {
                    if (One.ID == PeriodId)
                    {
                        if (Counter == 0)
                        {
                            flgLastPostedPeriod = true;
                        }
                        else
                        {
                            //LastPeriodId = One.ID - 1;
                            flgLastPostedPeriod = Convert.ToBoolean((from a in dbHrPayroll.CfgPeriodDates where a.ID == LastPeriodId select a.FlgLocked).FirstOrDefault());
                        }
                    }
                    LastPeriodId = One.ID;
                    Counter++;
                }
                //if (flgLastPostedPeriod)
                if (true)
                {
                    ProcessSalaryMFM();
                }
                else
                {
                    oApplication.StatusBar.SetText("Close Previos Month Processing.", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private Boolean postIntoSbo()
        {
            Boolean retValue = true;
            try
            {
                if (selJe == "")
                {
                    oApplication.SetStatusBarMessage("Select a JE draft to post");
                    return false;
                }
                int confirm = oApplication.MessageBox("Are you sure you want to post draft? ", 3, "Yes", "No", "Cancel");
                if (confirm == 2 || confirm == 3)
                {
                    retValue = false;
                    return retValue;
                }
                bool flgAlreadyPosted = (from a in dbHrPayroll.TrnsJE
                                         where a.ID.ToString() == selJe
                                         select a.FlgPosted).FirstOrDefault() ?? false;
                if (flgAlreadyPosted)
                {
                    oApplication.StatusBar.SetText("Journal entry already posted.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                TrnsJE je = (from p in dbHrPayroll.TrnsJE where p.ID.ToString() == selJe select p).FirstOrDefault();
                if (je == null)
                {
                    oApplication.StatusBar.SetText("Journal entry not found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return false;
                }
                DateTime PostingDateValue;
                string strResult = string.Empty;
                if (txtPostingDate.Value != "")
                {
                    PostingDateValue = DateTime.ParseExact(txtPostingDate.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    strResult = Program.objHrmsUI.postJe(je.ID, PostingDateValue);
                }
                else
                {
                    strResult = Program.objHrmsUI.postJe(je.ID);
                }
                //string strResult = "3232";
                if (strResult.Contains("Error"))
                {
                    oApplication.SetStatusBarMessage(strResult);
                }
                else
                {
                    je.SBOJeNum = Convert.ToInt32(strResult);
                    je.FlgPosted = true;
                    dbHrPayroll.SubmitChanges();
                    //doPostingTransactions(Convert.ToInt32(selJe));
                    AfterPostingTransactionAdvances(Convert.ToInt32(selJe));
                    AfterPostingTransactionLoans(Convert.ToInt32(selJe));
                    getPEmployees();
                }
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
                retValue = false;
            }
            return retValue;
        }

        private void cancelDraft()
        {
            try
            {
                if (selJe == "")
                {
                    oApplication.SetStatusBarMessage("Select a JE draft to delete");
                    return;
                }
                int confirm = oApplication.MessageBox("Are you sure you want to cancel draft? ", 3, "Yes", "No", "Cancel");
                if (confirm == 2 || confirm == 3) return;
                TrnsJE je = (from p in dbHrPayroll.TrnsJE where p.ID.ToString() == selJe.ToString() select p).FirstOrDefault();
                if (je == null) return;
                if (je != null && Convert.ToBoolean(Program.systemInfo.FlgA1Integration))
                {
                    VoidA1JERegister(je);
                }
                if (je != null && Convert.ToBoolean(Program.systemInfo.FlgJELocationWise))
                {
                    VoidSalaryClassRegister(je);
                }
                dbHrPayroll.TrnsJE.DeleteOnSubmit(je);
                IEnumerable<TrnsSalaryProcessRegister> salries = from p in dbHrPayroll.TrnsSalaryProcessRegister where p.JENum.ToString() == selJe select p;
                foreach (TrnsSalaryProcessRegister salary in salries)
                {
                    salary.JENum = null;
                    salary.JENumA1 = null;
                    salary.SalaryStatus = 0;
                }

                IEnumerable<TrnsJECCRegister> ocollection = from a in dbHrPayroll.TrnsJECCRegister where a.JEID.ToString() == selJe select a;

                foreach (TrnsJECCRegister oneline in ocollection)
                {
                    dbHrPayroll.TrnsJECCRegister.DeleteOnSubmit(oneline);
                }
                dbHrPayroll.SubmitChanges();
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
            }
            selJe = "";
            getPEmployees();
        }

        private void cancelDraftOriginal()
        {
            try
            {
                if (selJe == "")
                {
                    oApplication.SetStatusBarMessage("Select a JE draft to delete");
                    return;
                }
                int confirm = oApplication.MessageBox("Are you sure you want to cancel draft? ", 3, "Yes", "No", "Cancel");
                if (confirm == 2 || confirm == 3) return;
                TrnsJE je = (from p in dbHrPayroll.TrnsJE where p.ID.ToString() == selJe.ToString() select p).FirstOrDefault();
                if (je == null) return;
                if (je != null && Convert.ToBoolean(Program.systemInfo.FlgA1Integration))
                {
                    VoidA1JERegister(je);
                }
                dbHrPayroll.TrnsJE.DeleteOnSubmit(je);
                IEnumerable<TrnsSalaryProcessRegister> salries = from p in dbHrPayroll.TrnsSalaryProcessRegister where p.JENum.ToString() == selJe select p;
                foreach (TrnsSalaryProcessRegister salary in salries)
                {
                    salary.JENum = null;
                    salary.JENumA1 = null;
                    salary.SalaryStatus = 0;
                }

                IEnumerable<TrnsJECCRegister> ocollection = from a in dbHrPayroll.TrnsJECCRegister where a.JEID.ToString() == selJe select a;

                foreach (TrnsJECCRegister oneline in ocollection)
                {
                    dbHrPayroll.TrnsJECCRegister.DeleteOnSubmit(oneline);
                }
                dbHrPayroll.SubmitChanges();
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error occured " + ex.Message);
            }
            selJe = "";
            getPEmployees();
        }

        private void VoidA1JERegister(TrnsJE poDoc)
        {
            try
            {
                var oCollection = (from a in dbHrPayroll.TrnsSalaryProcessRegister where a.JENum == poDoc.ID select a).ToList();
                foreach (var One in oCollection)
                {
                    var A1JeLines = (from a in dbHrPayroll.TrnsJEA1 where a.DocNum == One.JENumA1 select a).FirstOrDefault();
                    if (A1JeLines != null)
                    {
                        dbHrPayroll.TrnsJEA1.DeleteOnSubmit(A1JeLines);
                    }
                }
                dbHrPayroll.SubmitChanges();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("VoidA1JERegister : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void VoidSalaryClassRegister(TrnsJE poDoc)
        {
            try
            {
                var oCollection = (from a in dbHrPayroll.TrnsSalaryProcessRegister where a.JENum == poDoc.ID select a).ToList();
                foreach (var One in oCollection)
                {
                    var oSalClassCollection = (from a in dbHrPayroll.TrnsSalaryClassification
                                               where a.EmpID == One.EmpID
                                               && a.PeriodID == One.PayrollPeriodID
                                               select a).ToList();

                    if (oSalClassCollection != null)
                    {
                        dbHrPayroll.TrnsSalaryClassification.DeleteAllOnSubmit(oSalClassCollection);
                    }
                }
                dbHrPayroll.SubmitChanges();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("VoidA1JERegister : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void PostSalaryCostCenter()
        {
            try
            {
                List<string> SLRIDList = new List<string>();
                int confirm = oApplication.MessageBox("JE posting is irr-reversable. Are you sure you want to post salary? ", 3, "Yes", "No", "Cancel");
                if (confirm == 2 || confirm == 3) return;
                CfgPeriodDates payrollperiod = (from p in dbHrPayroll.CfgPeriodDates where p.ID.ToString() == cbPeriod.Value.ToString().Trim() select p).FirstOrDefault();
                string spIds = "0";
                int totalCnt = 0;
                grdEmpPost.FlushToDataSource();
                for (int i = 0; i < dtEmpsPost.Rows.Count; i++)
                {
                    string sel = dtEmpsPost.GetValue("isSel", i);
                    if (sel == "Y")
                    {
                        if (totalCnt == 0)
                        {
                            spIds = dtEmpsPost.GetValue("id", i);
                        }
                        else
                        {
                            spIds += ", " + dtEmpsPost.GetValue("id", i);
                        }
                        SLRIDList.Add(dtEmpsPost.GetValue("id", i));
                        totalCnt++;
                    }
                }
                if (SLRIDList.Count == 0)
                {
                    oApplication.SetStatusBarMessage("Select employees to post ");
                }

                SearchKeyVal.Clear();
                SearchKeyVal.Add("spIds", spIds);
                string JeSql = sqlString.getSql("JEQueryCC", SearchKeyVal);
                foreach (string OneValue in SLRIDList)
                {
                    if (!string.IsNullOrEmpty(OneValue))
                    {
                        dbHrPayroll.CCGLPerEmployee(Convert.ToInt32(OneValue));
                    }
                }
                if (SLRIDList.Count > 0)
                {
                    DataTable dtJeDetail = ds.getDataTable(JeSql);
                    //DataTable dtJeDetail = new DataTable();
                    string errMsg = "";
                    string strCode = "";
                    string strName = "";
                    foreach (DataRow dr in dtJeDetail.Rows)
                    {
                        strCode = dr["AcctCode"].ToString();
                        strName = dr["AcctName"].ToString();
                        if (strCode == "Not Found")
                        {
                            errMsg = "GL Missing. Please confirm that GL Determination complete.";
                        }
                    }
                    if (errMsg != "")
                    {
                        oApplication.SetStatusBarMessage(errMsg);
                        return;
                    }
                    TrnsJE je = new TrnsJE();
                    je.CreateDt = DateTime.Now;
                    je.FlgCanceled = false;
                    je.FlgPosted = false;
                    je.JEPostingDate = payrollperiod.EndDate;
                    je.PayrollID = payrollperiod.CfgPayrollDefination.ID;
                    je.PeriodID = payrollperiod.ID;
                    je.Memo = " Payroll JE for period " + payrollperiod.PeriodName;

                    foreach (DataRow dr in dtJeDetail.Rows)
                    {
                        TrnsJEDetail jed = new TrnsJEDetail();
                        jed.AcctCode = dr["AcctCode"].ToString();
                        jed.AcctName = dr["AcctName"].ToString();
                        //jed.Debit = Convert.ToDecimal(dr["Debit"].ToString());
                        //jed.Credit = Convert.ToDecimal(dr["Credit"].ToString());
                        jed.Debit = mfmRoudingValues(Convert.ToDecimal(dr["Debit"].ToString()), RoundingSet);
                        jed.Credit = mfmRoudingValues(Convert.ToDecimal(dr["Credit"].ToString()), RoundingSet);
                        jed.CostCenter = dr["CostCenter"].ToString();
                        jed.Project = dr["Project"].ToString();
                        je.TrnsJEDetail.Add(jed);
                    }
                    dbHrPayroll.TrnsJE.InsertOnSubmit(je);
                    dbHrPayroll.SubmitChanges();
                    int jeNum = je.ID;

                    for (int i = 0; i < dtEmpsPost.Rows.Count; i++)
                    {
                        string sel = dtEmpsPost.GetValue("isSel", i);
                        string id = dtEmpsPost.GetValue("id", i);
                        if (sel == "Y")
                        {
                            string processId = dtEmpsPost.GetValue("id", i);
                            TrnsSalaryProcessRegister sp = (from p in dbHrPayroll.TrnsSalaryProcessRegister where p.Id.ToString() == processId select p).Single();
                            IEnumerable<TrnsJECCRegister> ocollection = from a in dbHrPayroll.TrnsJECCRegister where a.SalaryID.ToString() == processId select a;
                            sp.JENum = jeNum;
                            sp.SalaryStatus = 1;
                            foreach (TrnsJECCRegister oneline in ocollection)
                            {
                                oneline.JEID = jeNum;
                            }
                        }
                    }
                    dbHrPayroll.SubmitChanges();
                    getPEmployees();
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("PostSalaryCostCenter Exception : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void PostA1IntegrationJE()
        {
            try
            {
                string Period = "";
                Int32 DocNum = 0;
                List<string> oEmpID = new List<string>();
                Period = cbPeriod.Value.Trim();
                CfgPeriodDates oPeriod = (from a in dbHrPayroll.CfgPeriodDates where a.ID.ToString() == Period select a).FirstOrDefault();
                int totalCnt = 0;
                string spIds = "0";
                grdEmpPost.FlushToDataSource();
                for (int i = 0; i < dtEmpsPost.Rows.Count; i++)
                {
                    string sel = dtEmpsPost.GetValue("isSel", i);
                    if (sel == "Y")
                    {
                        if (totalCnt == 0)
                        {
                            spIds = dtEmpsPost.GetValue("id", i);
                            oEmpID.Add(dtEmpsPost.GetValue("id", i));
                        }
                        else
                        {
                            spIds += ", " + dtEmpsPost.GetValue("id", i);
                            oEmpID.Add(dtEmpsPost.GetValue("id", i));
                        }
                        totalCnt++;

                    }
                }
                if (spIds == "0")
                {
                    oApplication.SetStatusBarMessage("Select employees to post ");
                }
                SearchKeyVal.Clear();
                SearchKeyVal.Add("spIds", spIds);
                string JeSql = sqlString.getSql("JEQueryA1", SearchKeyVal);
                if (totalCnt > 0)
                {
                    DataTable dtJeDetail = ds.getDataTable(JeSql);

                    string errMsg = "";
                    string strCode = "";
                    string strName = "";
                    foreach (DataRow dr in dtJeDetail.Rows)
                    {
                        strCode = dr["AcctCode"].ToString();
                        strName = dr["AcctName"].ToString();
                        if (strCode == "Not Found")
                        {
                            errMsg = "GL Missing. Please confirm that GL Determination complete.";
                        }
                    }
                    if (errMsg != "")
                    {
                        oApplication.StatusBar.SetText(errMsg, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return;
                    }
                    int lastdoc = 0;
                    lastdoc = Convert.ToInt32((from a in dbHrPayroll.TrnsJEA1 select a.DocNum).Max());
                    if (lastdoc == 0)
                    {
                        DocNum = 1;
                    }
                    else
                    {
                        DocNum = lastdoc + 1;
                    }
                    string PreviousReference = "";
                    bool flgFirstTime = true;
                    foreach (DataRow dr in dtJeDetail.Rows)
                    {
                        TrnsJEA1 oDoc = new TrnsJEA1();
                        dbHrPayroll.TrnsJEA1.InsertOnSubmit(oDoc);
                        //if (flgFirstTime && PreviousReference != Convert.ToString(dr["Reference"]))
                        //{
                        //    oDoc.DocNum = DocNum;
                        //    flgFirstTime = false;
                        //    PreviousReference = Convert.ToString(dr["Reference"]);
                        //}
                        //else if (!flgFirstTime && PreviousReference != Convert.ToString(dr["Reference"]))
                        //{
                        //    oDoc.DocNum = DocNum + 1;
                        //    PreviousReference = Convert.ToString(dr["Reference"]);
                        //}
                        //else
                        //{
                        //    oDoc.DocNum = DocNum;
                        //}
                        oDoc.DocNum = DocNum;
                        oDoc.DocDate = Convert.ToDateTime(oPeriod.EndDate);
                        oDoc.PostingDate = Convert.ToDateTime(oPeriod.EndDate);
                        oDoc.Reference = Convert.ToString(dr["Reference"]);
                        oDoc.PeriodName = Convert.ToString(oPeriod.PeriodName);
                        oDoc.CompanyCode = Convert.ToString(dr["CompCode"]);
                        oDoc.DocType = "SA";
                        oDoc.Currency = Convert.ToString(dr["Currency"]);
                        oDoc.AcctType = Convert.ToString(dr["AcctType"]);
                        oDoc.GLCode = Convert.ToString(dr["AcctCode"]);
                        oDoc.GLIndication = Convert.ToString(dr["SpecialGLIndicator"]);
                        oDoc.AcctTypeDC = Convert.ToString(dr["GLAcctType"]);
                        oDoc.Amount = Convert.ToDecimal(dr["Amount"]);
                        string CCCode = Convert.ToString(dr["CostCenter"]);
                        string CCDesc = "";
                        if (string.IsNullOrEmpty(CCCode))
                        {
                            oDoc.CCCode = "";
                            oDoc.CCDesc = "";
                        }
                        else
                        {
                            oDoc.CCCode = CCCode.Trim();
                            string strSql = "Select \"PrcCode\", \"PrcName\" From \"OPRC\" Where \"PrcCode\"= '" + CCCode.Trim() + "'";
                            SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                            oRecSet.DoQuery(strSql);
                            while (oRecSet.EoF == false)
                            {
                                CCDesc = Convert.ToString(oRecSet.Fields.Item("PrcName").Value);
                                oRecSet.MoveNext();
                            }
                            if (string.IsNullOrEmpty(CCDesc))
                            {
                                oDoc.CCDesc = "";
                            }
                            else
                            {
                                oDoc.CCDesc = CCDesc;
                            }
                        }
                        oDoc.ValueDate = Convert.ToDateTime(oPeriod.EndDate);
                        oDoc.ProfitCenter = Convert.ToString(dr["ProfitCenter"]);
                        oDoc.CreatedBy = oCompany.UserName;
                        oDoc.CreateDt = DateTime.Now;
                    }
                    dbHrPayroll.SubmitChanges();
                    foreach (var One in oEmpID)
                    {
                        TrnsSalaryProcessRegister oDoc = (from a in dbHrPayroll.TrnsSalaryProcessRegister where a.Id.ToString() == One select a).FirstOrDefault();
                        if (oDoc == null) continue;
                        oDoc.JENumA1 = DocNum;
                    }
                    dbHrPayroll.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("PostA1IntegrationJE Ex : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void PostSalary()
        {

            CfgPeriodDates payrollperiod = (from p in dbHrPayroll.CfgPeriodDates where p.ID.ToString() == cbPeriod.Value.ToString().Trim() select p).FirstOrDefault();

            int totalCnt = 0;
            string spIds = "0";
            grdEmpPost.FlushToDataSource();
            for (int i = 0; i < dtEmpsPost.Rows.Count; i++)
            {
                string sel = dtEmpsPost.GetValue("isSel", i);
                if (sel == "Y")
                {
                    if (totalCnt == 0)
                    {
                        spIds = dtEmpsPost.GetValue("id", i);
                    }
                    else
                    {
                        spIds += ", " + dtEmpsPost.GetValue("id", i);
                    }
                    totalCnt++;

                }
            }
            if (spIds == "0")
            {
                oApplication.SetStatusBarMessage("Select employees to post ");
            }
            SearchKeyVal.Clear();
            SearchKeyVal.Add("spIds", spIds);
            string JeSql = sqlString.getSql("JEQuery", SearchKeyVal);

            if (totalCnt > 0)
            {
                DataTable dtJeDetail = ds.getDataTable(JeSql);

                string errMsg = "";
                string strCode = "";
                string strName = "";
                foreach (DataRow dr in dtJeDetail.Rows)
                {
                    strCode = dr["AcctCode"].ToString();
                    strName = dr["AcctName"].ToString();
                    if (strCode == "Not Found" || string.IsNullOrEmpty(strCode))
                    {
                        errMsg = "GL Missing. Please confirm that GL Determination complete.";
                    }

                }
                if (errMsg != "")
                {
                    oApplication.SetStatusBarMessage(errMsg);
                    return;
                }
                TrnsJE je = new TrnsJE();
                je.CreateDt = DateTime.Now;
                je.FlgCanceled = false;
                je.FlgPosted = false;
                je.JEPostingDate = payrollperiod.EndDate;
                je.PayrollID = payrollperiod.CfgPayrollDefination.ID;
                je.PeriodID = payrollperiod.ID;
                je.Memo = " Payroll JE for period " + payrollperiod.PeriodName;

                foreach (DataRow dr in dtJeDetail.Rows)
                {
                    TrnsJEDetail jed = new TrnsJEDetail();
                    jed.AcctCode = dr["AcctCode"].ToString();
                    jed.AcctName = dr["AcctName"].ToString();
                    //jed.Debit = Convert.ToDecimal(dr["Debit"].ToString());
                    //jed.Credit = Convert.ToDecimal(dr["Credit"].ToString());
                    jed.Debit = mfmRoudingValues(Convert.ToDecimal(dr["Debit"].ToString()), RoundingSet);
                    jed.Credit = mfmRoudingValues(Convert.ToDecimal(dr["Credit"].ToString()), RoundingSet);
                    jed.CostCenter = dr["CostCenter"].ToString();
                    jed.Project = dr["Project"].ToString();
                    jed.FCurrency = dr["EmpCurr"].ToString();
                    je.TrnsJEDetail.Add(jed);
                }
                dbHrPayroll.TrnsJE.InsertOnSubmit(je);
                dbHrPayroll.SubmitChanges();
                int jeNum = je.ID;

                for (int i = 0; i < dtEmpsPost.Rows.Count; i++)
                {
                    string sel = dtEmpsPost.GetValue("isSel", i);
                    string id = dtEmpsPost.GetValue("id", i);
                    if (sel == "Y")
                    {
                        string processId = dtEmpsPost.GetValue("id", i);
                        TrnsSalaryProcessRegister sp = (from p in dbHrPayroll.TrnsSalaryProcessRegister where p.Id.ToString() == processId select p).Single();
                        sp.JENum = jeNum;
                        sp.SalaryStatus = 1;

                    }

                }
                dbHrPayroll.SubmitChanges();

                getPEmployees();
            }

        }

        private void PostSalaryClassified()
        {
            CfgPeriodDates payrollperiod = (from p in dbHrPayroll.CfgPeriodDates where p.ID.ToString() == cbPeriod.Value.ToString().Trim() select p).FirstOrDefault();
            int totalCnt = 0;
            string spIds = "0";
            grdEmpPost.FlushToDataSource();
            for (int i = 0; i < dtEmpsPost.Rows.Count; i++)
            {
                string sel = dtEmpsPost.GetValue("isSel", i);
                if (sel == "Y")
                {
                    if (totalCnt == 0)
                    {
                        spIds = dtEmpsPost.GetValue("empId", i);
                    }
                    else
                    {
                        spIds += ", " + dtEmpsPost.GetValue("empId", i);
                    }
                    totalCnt++;

                }
            }
            if (spIds == "0")
            {
                oApplication.SetStatusBarMessage("Select employees to post ");
            }
            SearchKeyVal.Clear();
            SearchKeyVal.Add("EmpID", spIds);
            SearchKeyVal.Add("PeriodID", cbPeriod.Value.Trim());
            string JeSql = sqlString.getSql("JEQueryClass", SearchKeyVal);

            if (totalCnt > 0)
            {
                DataTable dtJeDetail = ds.getDataTable(JeSql);

                string errMsg = "";
                string strCode = "";
                string strName = "";
                foreach (DataRow dr in dtJeDetail.Rows)
                {
                    strCode = dr["AcctCode"].ToString();
                    strName = dr["AcctName"].ToString();
                    if (strCode == "Not Found" || string.IsNullOrEmpty(strCode))
                    {
                        errMsg = "GL Missing. Please confirm that GL Determination complete.";
                    }

                }
                if (errMsg != "")
                {
                    oApplication.SetStatusBarMessage(errMsg);
                    return;
                }
                TrnsJE je = new TrnsJE();
                je.CreateDt = DateTime.Now;
                je.FlgCanceled = false;
                je.FlgPosted = false;
                je.JEPostingDate = payrollperiod.EndDate;
                je.PayrollID = payrollperiod.CfgPayrollDefination.ID;
                je.PeriodID = payrollperiod.ID;
                je.Memo = " Payroll JE for period " + payrollperiod.PeriodName;

                foreach (DataRow dr in dtJeDetail.Rows)
                {
                    if (Convert.ToBoolean(Program.systemInfo.FlgJELocationWise))
                    {
                        MstLocation oLocation = (from l in dbHrPayroll.MstLocation
                                                 where l.Id == Convert.ToInt32(dr["Location"].ToString())
                                                 select l).FirstOrDefault();

                        TrnsJEDetail jed = new TrnsJEDetail();
                        jed.AcctCode = dr["AcctCode"].ToString() + '-' + oLocation.Name;
                        jed.AcctName = dr["AcctName"].ToString();
                        //jed.Debit = Convert.ToDecimal(dr["Debit"].ToString());
                        //jed.Credit = Convert.ToDecimal(dr["Credit"].ToString());
                        jed.Debit = mfmRoudingValues(Convert.ToDecimal(dr["Debit"].ToString()), RoundingSet);
                        jed.Credit = mfmRoudingValues(Convert.ToDecimal(dr["Credit"].ToString()), RoundingSet);
                        jed.CostCenter = dr["CostCenter"].ToString();
                        jed.LocationID = Convert.ToInt32(dr["Location"].ToString());
                        je.TrnsJEDetail.Add(jed);

                    }
                    else
                    {
                        TrnsJEDetail jed = new TrnsJEDetail();
                        jed.AcctCode = dr["AcctCode"].ToString();
                        jed.AcctName = dr["AcctName"].ToString();
                        //jed.Debit = Convert.ToDecimal(dr["Debit"].ToString());
                        //jed.Credit = Convert.ToDecimal(dr["Credit"].ToString());
                        jed.Debit = mfmRoudingValues(Convert.ToDecimal(dr["Debit"].ToString()), RoundingSet);
                        jed.Credit = mfmRoudingValues(Convert.ToDecimal(dr["Credit"].ToString()), RoundingSet);
                        jed.CostCenter = dr["CostCenter"].ToString();
                        je.TrnsJEDetail.Add(jed);
                    }
                }
                dbHrPayroll.TrnsJE.InsertOnSubmit(je);
                dbHrPayroll.SubmitChanges();
                int jeNum = je.ID;

                for (int i = 0; i < dtEmpsPost.Rows.Count; i++)
                {
                    string sel = dtEmpsPost.GetValue("isSel", i);
                    string id = dtEmpsPost.GetValue("id", i);
                    if (sel == "Y")
                    {
                        string processId = dtEmpsPost.GetValue("id", i);
                        TrnsSalaryProcessRegister sp = (from p in dbHrPayroll.TrnsSalaryProcessRegister where p.Id.ToString() == processId select p).Single();
                        sp.JENum = jeNum;
                        sp.SalaryStatus = 1;

                    }

                }
                dbHrPayroll.SubmitChanges();

                getPEmployees();
            }

        }

        private void ExportTosifOld()
        {
            try
            {

                int totalCnt = 0;
                string spIds = "0";
                int salarycheck = 0, sid = 0;
                grdEmpPost.FlushToDataSource();
                for (int i = 0; i < dtEmpsPost.Rows.Count; i++)
                {
                    string sel = dtEmpsPost.GetValue("isSel", i);
                    if (sel == "Y")
                    {
                        if (totalCnt == 0)
                        {
                            spIds = dtEmpsPost.GetValue("id", i);
                        }
                        else
                        {
                            spIds += ", " + dtEmpsPost.GetValue("id", i);
                        }
                        sid = Convert.ToInt32(dtEmpsPost.GetValue("id", i));
                        salarycheck = (from a in dbHrPayroll.TrnsSalaryProcessRegister where a.Id == sid select a.SalaryStatus).FirstOrDefault() == null ? 0 : Convert.ToInt32((from a in dbHrPayroll.TrnsSalaryProcessRegister where a.Id == sid select a.SalaryStatus).FirstOrDefault());
                        totalCnt++;

                    }
                }
                if (spIds == "0")
                {
                    oApplication.SetStatusBarMessage("Select employees to Export SIF");
                }
                else
                {

                    #region PeriodReports

                    if (salarycheck == 1)
                    {
                        if (!String.IsNullOrEmpty(cbPayroll.Value))
                        {
                            String PeriodCrit = "";
                            String Period = "";
                            String Critaria = "";
                            TblRpts oReport = (from a in dbHrPayroll.TblRpts where a.RptCode == "WPS Sheet" select a).FirstOrDefault();
                            Int32 selectedperiodid = Convert.ToInt32(cbPeriod.Selected.Value);
                            Int32 selectedpayroll = Convert.ToInt32(cbPayroll.Selected.Value);
                            var oPeriodCurrent = (from a in dbHrPayroll.CfgPeriodDates where a.ID == selectedperiodid select a).FirstOrDefault();
                            Critaria += " Where 1=1 ";
                            if (Convert.ToBoolean(oReport.FlgPeriod))
                            {
                                if (String.IsNullOrEmpty(cbPeriod.Value))
                                {
                                    oApplication.StatusBar.SetText("Select Payroll Period", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    return;
                                }
                                Period = cbPeriod.Selected.Description;
                                //string cri = " Where TrnsSalaryProcessRegister.Id in (" + spIds + ")";
                                PeriodCrit += " AND PeriodName = '" + Period + "' and EmployeeID in (" + spIds + ")";
                                Critaria += PeriodCrit;
                            }


                            int cnt = (from p in dbHrPayroll.TblRpts where p.RptCode == "WPS Sheet" select p).Count();
                            if (cnt > 0)
                            {

                                var oEmp = (from a in dbHrPayroll.MstEmployee where a.FlgActive == true select a).FirstOrDefault();
                                byte[] rptBytes = oReport.RptFileStr.ToArray();
                                FileStream fs = new FileStream(Application.StartupPath + "\\test.rpt", System.IO.FileMode.Create);
                                int len = rptBytes.Length;
                                fs.Write(rptBytes, 0, len);
                                fs.Flush();
                                fs.Close();

                                ReportDocument report = new ReportDocument();

                                report.Load(Application.StartupPath + "\\test.rpt");

                                //report.SetDatabaseLogon(Program.objHrmsUI.HRMSDBuid, Program.objHrmsUI.HRMSDbPwd);
                                //report.SetDatabaseLogon(Program.objHrmsUI.HRMSDBuid, Program.objHrmsUI.HRMSDbPwd,
                                //    Program.objHrmsUI.HRMSDbServer, Program.objHrmsUI.HRMSDbName);


                                report.SetParameterValue(0, Critaria);
                                ExportOptions exportOpts = new ExportOptions();
                                DiskFileDestinationOptions diskOpts = new DiskFileDestinationOptions();
                                CharacterSeparatedValuesFormatOptions FormatOpts = new CharacterSeparatedValuesFormatOptions();
                                ExportFormatType Exportformat = ExportFormatType.NoFormat;
                                exportOpts = report.ExportOptions;
                                exportOpts.FormatOptions = FormatOpts;



                                // Set the excel format options.

                                string strPath = Application.StartupPath + "\\" + Convert.ToString(oEmp.MedicalCardExpDt) + DateTime.Now.ToString("yyMMddHHmmss") + ".txt";
                                //string strPath = Application.StartupPath + "\\" + Convert.ToString(oEmp.MedicalCardExpDt) + DateTime.Now.ToString("yyMMddHHmmss") + ".doc";
                                diskOpts.DiskFileName = strPath;
                                exportOpts.DestinationOptions = diskOpts;
                                Exportformat = ExportFormatType.Excel;
                                report.ExportToDisk(Exportformat, strPath);

                                #region AR SIF Conversion
                                FileInfo f = new FileInfo(strPath);

                                f.MoveTo(Path.ChangeExtension(strPath, ".SIF"));
                                //DirectoryInfo dir = new DirectoryInfo(Application.StartupPath);
                                //string[] files = Directory.GetFiles(Application.StartupPath, "*.xls", SearchOption.AllDirectories);

                                //foreach (string file in files)
                                //{
                                //    // However you want to process the CSV file
                                //    string filename = Path.ChangeExtension(file, ".xls");
                                //    System.IO.File.Move(filename, filename + ".SIF");
                                //    //files.CopyTo(file,
                                //}
                                //strPath = Application.StartupPath;
                                //f.MoveTo(Path.ChangeExtension(strPath, ".SIF"));
                                #endregion
                            }
                        }
                        else
                        {
                            oApplication.StatusBar.SetText("Select Payroll Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return;
                        }

                    }

                    else
                    {
                        oApplication.StatusBar.SetText("Only Posted Employees export to sif", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return;

                    }


                    #endregion
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("ExportTosif : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void ExportTosif()
        {
            try
            {

                int totalCnt = 0;
                string spIds = "0";
                int salarycheck = 0, sid = 0;
                grdEmpPost.FlushToDataSource();
                for (int i = 0; i < dtEmpsPost.Rows.Count; i++)
                {
                    string sel = dtEmpsPost.GetValue("isSel", i);
                    if (sel == "Y")
                    {
                        if (totalCnt == 0)
                        {
                            spIds = dtEmpsPost.GetValue("id", i);
                        }
                        else
                        {
                            spIds += ", " + dtEmpsPost.GetValue("id", i);
                        }
                        sid = Convert.ToInt32(dtEmpsPost.GetValue("id", i));
                        salarycheck = (from a in dbHrPayroll.TrnsSalaryProcessRegister where a.Id == sid select a.SalaryStatus).FirstOrDefault() == null ? 0 : Convert.ToInt32((from a in dbHrPayroll.TrnsSalaryProcessRegister where a.Id == sid select a.SalaryStatus).FirstOrDefault());
                        totalCnt++;

                    }
                }
                if (spIds == "0")
                {
                    oApplication.SetStatusBarMessage("Select employees to Export SIF");
                }
                else
                {

                    #region PeriodReports

                    if (salarycheck == 1)
                    {
                        if (!String.IsNullOrEmpty(cbPayroll.Value))
                        {
                            String PeriodCrit = "";
                            String Period = "";
                            String Critaria = "";
                            TblRpts oReport = (from a in dbHrPayroll.TblRpts where a.RptCode == "WPS Sheet" select a).FirstOrDefault();
                            Int32 selectedperiodid = Convert.ToInt32(cbPeriod.Selected.Value);
                            Int32 selectedpayroll = Convert.ToInt32(cbPayroll.Selected.Value);
                            var oPeriodCurrent = (from a in dbHrPayroll.CfgPeriodDates where a.ID == selectedperiodid select a).FirstOrDefault();
                            Critaria += " Where 1=1 ";
                            if (Convert.ToBoolean(oReport.FlgPeriod))
                            {
                                if (String.IsNullOrEmpty(cbPeriod.Value))
                                {
                                    oApplication.StatusBar.SetText("Select Payroll Period", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    return;
                                }
                                Period = cbPeriod.Selected.Description;
                                //string cri = " Where TrnsSalaryProcessRegister.Id in (" + spIds + ")";
                                PeriodCrit += " AND PeriodName = '" + Period + "' and EmployeeID in (" + spIds + ")";
                                Critaria += PeriodCrit;
                            }


                            int cnt = (from p in dbHrPayroll.TblRpts where p.RptCode == "WPS Sheet" select p).Count();
                            if (cnt > 0)
                            {

                                var oEmp = (from a in dbHrPayroll.MstEmployee where a.FlgActive == true select a).FirstOrDefault();
                                byte[] rptBytes = oReport.RptFileStr.ToArray();
                                FileStream fs = new FileStream(System.Windows.Forms.Application.StartupPath + "\\test.rpt", System.IO.FileMode.Create);
                                int len = rptBytes.Length;
                                fs.Write(rptBytes, 0, len);
                                fs.Flush();
                                fs.Close();

                                ReportDocument report = new ReportDocument();

                                report.Load(System.Windows.Forms.Application.StartupPath + "\\test.rpt");
                                Program.SetReport(report);
                                report.SetParameterValue(0, Critaria);
                                ExportOptions exportOpts = new ExportOptions();
                                DiskFileDestinationOptions diskOpts = new DiskFileDestinationOptions();
                                CharacterSeparatedValuesFormatOptions FormatOpts = new CharacterSeparatedValuesFormatOptions();
                                ExportFormatType Exportformat = ExportFormatType.NoFormat;
                                exportOpts = report.ExportOptions;
                                exportOpts.FormatOptions = FormatOpts;

                                string strPath = System.Windows.Forms.Application.StartupPath + "\\" + Convert.ToString(oEmp.MedicalCardExpDt) + DateTime.Now.ToString("yyMMddHHmmss") + ".xls";
                                if (!Directory.Exists(Path.Combine(System.Windows.Forms.Application.StartupPath, "CSV")))
                                    Directory.CreateDirectory(Path.Combine(System.Windows.Forms.Application.StartupPath, "CSV"));
                                string strPathto = System.Windows.Forms.Application.StartupPath + "\\" + "CSV" + "\\" + Convert.ToString(oEmp.MedicalCardExpDt) + DateTime.Now.ToString("yyMMddHHmmss") + ".xls";
                                diskOpts.DiskFileName = strPath;
                                exportOpts.DestinationOptions = diskOpts;
                                Exportformat = ExportFormatType.Excel;
                                report.ExportToDisk(Exportformat, strPath);


                                #region AR SIF Conversion

                                String fromFile = strPath;
                                String toFile = strPathto;
                                Microsoft.Office.Interop.Excel.Application app = new Microsoft.Office.Interop.Excel.Application();
                                Microsoft.Office.Interop.Excel.Workbook wb = app.Workbooks.Open(fromFile, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);

                                wb.SaveAs(toFile, Microsoft.Office.Interop.Excel.XlFileFormat.xlCSVWindows, Type.Missing, Type.Missing, false, false, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, Microsoft.Office.Interop.Excel.XlSaveConflictResolution.xlLocalSessionChanges, false, Type.Missing, Type.Missing, Type.Missing);


                                wb.Close(false);

                                app.Quit();
                                string path = strPathto;

                                string[] lines = File.ReadAllLines(path);
                                for (int i = 0; i < lines.Length; i++)
                                {
                                    string line = lines[i];
                                    if (line.Contains(",,,,,,,,,"))
                                    {
                                        line = "";
                                        lines[i] = "";
                                    }
                                }
                                File.WriteAllLines(strPathto, lines);
                                FileInfo f = new FileInfo(toFile);

                                f.MoveTo(Path.ChangeExtension(toFile, ".SIF"));
                                if (File.Exists(strPath))
                                {
                                    File.Delete(strPath);
                                }
                                #endregion
                            }
                        }
                        else
                        {
                            oApplication.StatusBar.SetText("Select Payroll Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return;
                        }
                    }
                    else
                    {
                        oApplication.StatusBar.SetText("Only Posted Employees export to sif", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return;
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("ExportTosif : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private decimal mfmRoudingValues(decimal pValue, int pDegree)
        {
            try
            {
                if (pDegree == 1)
                {
                    return Math.Round(pValue, 0);
                }
                else
                {
                    return pValue;
                }
            }
            catch (Exception ex)
            {
                return pValue;
                //oApplication.StatusBar.SetText("Precision Error  : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private CfgPeriodDates GetPreviousPeriod(CfgPayrollDefination oPayroll, CfgPeriodDates oCurrentPeriod)
        {
            CfgPeriodDates oPreviosPeriod = null;
            try
            {

                //var oFiscal = (from a in dbHrPayroll.MstCalendar
                //               where (a.FlgActive != null ? a.FlgActive : false) == true
                //               select a).FirstOrDefault();
                //if (oFiscal == null) return oPreviosPeriod;
                var oCollection = (from a in dbHrPayroll.CfgPeriodDates
                                   where a.PayrollId == oPayroll.ID
                                   //&& a.CalCode == oFiscal.Code
                                   select a).ToList();
                if (oCollection.Count == 0) return oPreviosPeriod;
                int PreviousID = 0;
                bool IDFound = false;
                foreach (var One in oCollection)
                {
                    if (oCurrentPeriod.ID == One.ID)
                    {
                        if (PreviousID == 0)
                        {
                            IDFound = true;
                            PreviousID = One.ID;
                        }
                        else
                        {
                            IDFound = true;
                        }
                    }
                    else
                    {
                        PreviousID = One.ID;
                    }
                    if (IDFound) break;
                }
                if (PreviousID != 0)
                {
                    oPreviosPeriod = (from a in dbHrPayroll.CfgPeriodDates
                                      where a.ID == PreviousID
                                      select a).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                logger(ex);
                oPreviosPeriod = null;
            }
            return oPreviosPeriod;
        }

        private void ClassifyAlreadyProcessedSalary()
        {
            try
            {
                using (dbHRMS oDBPrivate = new dbHRMS(Program.ConStrHRMS))
                {
                    var oPeriod = (from p in dbHrPayroll.CfgPeriodDates
                                   where p.ID.ToString() == cbPeriod.Value.ToString().Trim()
                                   select p).FirstOrDefault();
                    grdEmpPost.FlushToDataSource();
                    for (int i = 0; i < dtEmpsPost.Rows.Count; i++)
                    {
                        string sel = "", empcode = "";
                        sel = dtEmpsPost.GetValue("isSel", i);
                        empcode = dtEmpsPost.GetValue("empId", i);
                        if (sel.Trim().ToUpper() == "Y")
                        {
                            var oEmp = (from a in oDBPrivate.MstEmployee
                                        where a.EmpID == empcode
                                        select a).FirstOrDefault();
                            var oProcessSalary = (from a in oDBPrivate.TrnsSalaryProcessRegister
                                                  where a.MstEmployee.EmpID == empcode
                                                  && a.PayrollPeriodID == oPeriod.ID
                                                  && a.SalaryStatus == 0
                                                  select a).FirstOrDefault();
                            if (oProcessSalary != null)
                            {
                                var oData = oDBPrivate.MfmGetSalaryStatus(oEmp.ID.ToString(), oPeriod.ID).ToList();
                                foreach (var Oneline in oData)
                                {
                                    int LocationID = Convert.ToInt32(Oneline.LocationID);
                                    MstGLDetermination oGL = ds.getLocationGL(LocationID);

                                    #region Basic Salary
                                    var oBasicDetail = (from a in oDBPrivate.TrnsSalaryProcessRegisterDetail
                                                        where a.SRID == oProcessSalary.Id
                                                        && a.LineType == "BS"
                                                        select a).FirstOrDefault();
                                    TrnsSalaryClassification oRec = new TrnsSalaryClassification();
                                    oDBPrivate.TrnsSalaryClassification.InsertOnSubmit(oRec);
                                    oRec.EmpID = oEmp.ID;
                                    oRec.PeriodID = oPeriod.ID;
                                    oRec.Location = LocationID;
                                    oRec.DAccountCode = oGL.BasicSalary;
                                    oRec.DAccountDesc = Program.objHrmsUI.getAcctName(oGL.BasicSalary);
                                    oRec.CAccountCode = oGL.BSPayable;
                                    oRec.CAccountDesc = Program.objHrmsUI.getAcctName(oGL.BSPayable);
                                    oRec.LineValue = oBasicDetail.LineValue * Oneline.FD;
                                    oRec.LocationID = oGL.GLValue;

                                    #endregion

                                    #region Absents

                                    var oAbsentDetail = (from a in oDBPrivate.TrnsSalaryProcessRegisterDetail
                                                         where a.SRID == oProcessSalary.Id
                                                         && a.LineType == "Absent"
                                                         select a).ToList();
                                    foreach (var oSalLine in oAbsentDetail)
                                    {
                                        Hashtable elementGls = new Hashtable();
                                        var oEmpLeave = (from a in oDBPrivate.TrnsLeavesRequest
                                                         where a.EmpID == oEmp.ID
                                                         && a.ID == oSalLine.LineBaseEntry
                                                         select a).FirstOrDefault();

                                        var oLevDed = (from a in oDBPrivate.MstLeaveDeduction
                                                       where a.Code == oEmpLeave.DeductId
                                                       select a).FirstOrDefault();
                                        elementGls = ds.getLeaveDedGL(oEmp, oLevDed.Id);

                                        TrnsSalaryClassification oAbsentLine = new TrnsSalaryClassification();
                                        oDBPrivate.TrnsSalaryClassification.InsertOnSubmit(oAbsentLine);
                                        oAbsentLine.EmpID = oEmp.ID;
                                        oAbsentLine.PeriodID = oPeriod.ID;
                                        oAbsentLine.Location = LocationID;
                                        oAbsentLine.DAccountCode = elementGls["DrAcct"].ToString();
                                        oAbsentLine.DAccountDesc = elementGls["DrAcctName"].ToString();
                                        oAbsentLine.CAccountCode = elementGls["CrAcct"].ToString(); ;
                                        oAbsentLine.CAccountDesc = elementGls["CrAcctName"].ToString();
                                        //oAbsentLine.LocationID = Convert.ToInt32(elementGls["LocationID"].ToString());

                                        oAbsentLine.LineValue = oSalLine.LineValue * Oneline.FD;
                                    }
                                    #endregion

                                    #region Elements

                                    var oElemDetail = (from a in oDBPrivate.TrnsSalaryProcessRegisterDetail
                                                       where a.SRID == oProcessSalary.Id
                                                       && a.LineType == "Element"
                                                       select a).ToList();
                                    foreach (var OneElem in oElemDetail)
                                    {
                                        Hashtable elementGls = new Hashtable();
                                        var oEmpElement = (from a in oDBPrivate.TrnsEmployeeElementDetail
                                                           where a.TrnsEmployeeElement.EmployeeId == oEmp.ID
                                                           && a.Id == OneElem.LineBaseEntry
                                                           select a).FirstOrDefault();
                                        elementGls = ds.getElementGL(oEmp, oEmpElement.MstElements, oGL);
                                        TrnsSalaryClassification oElem = new TrnsSalaryClassification();
                                        oDBPrivate.TrnsSalaryClassification.InsertOnSubmit(oElem);
                                        oElem.EmpID = oEmp.ID;
                                        oElem.PeriodID = oPeriod.ID;
                                        oElem.Location = LocationID;
                                        oElem.DAccountCode = elementGls["DrAcct"].ToString();
                                        oElem.DAccountDesc = elementGls["DrAcctName"].ToString();
                                        oElem.CAccountCode = elementGls["CrAcct"].ToString(); ;
                                        oElem.CAccountDesc = elementGls["CrAcctName"].ToString();
                                        //oElem.LocationID = Convert.ToInt32(elementGls["LocationID"].ToString());
                                        oElem.LineValue = OneElem.LineValue * Oneline.FD;
                                    }

                                    #endregion

                                    #region Overtime

                                    var oOTDetail = (from a in oDBPrivate.TrnsSalaryProcessRegisterDetail
                                                     where a.SRID == oProcessSalary.Id
                                                     && a.LineType == "Over Time"
                                                     select a).ToList();
                                    foreach (var oSalLine in oOTDetail)
                                    {
                                        Hashtable elementGls = new Hashtable();
                                        var oEMPOT = (from a in oDBPrivate.MstOverTime
                                                      where a.ID == oSalLine.LineBaseEntry
                                                      select a).FirstOrDefault();
                                        elementGls = ds.getOverTimeGLClassified(oEmp, oEMPOT, LocationID);
                                        TrnsSalaryClassification oOTLine = new TrnsSalaryClassification();
                                        oDBPrivate.TrnsSalaryClassification.InsertOnSubmit(oOTLine);
                                        oOTLine.EmpID = oEmp.ID;
                                        oOTLine.PeriodID = oPeriod.ID;
                                        oOTLine.Location = LocationID;
                                        oOTLine.DAccountCode = elementGls["DrAcct"].ToString();
                                        oOTLine.DAccountDesc = elementGls["DrAcctName"].ToString();
                                        oOTLine.CAccountCode = elementGls["CrAcct"].ToString(); ;
                                        oOTLine.CAccountDesc = elementGls["CrAcctName"].ToString();
                                        //oOTLine.LocationID = Convert.ToInt32(elementGls["LocationID"].ToString());
                                        oOTLine.LineValue = oSalLine.LineValue * Oneline.FD;
                                    }

                                    #endregion

                                    #region Advance

                                    var oAdvanceDetail = (from a in oDBPrivate.TrnsSalaryProcessRegisterDetail
                                                          where a.SRID == oProcessSalary.Id
                                                          && a.LineType == "Advance Recovery"
                                                          select a).ToList();
                                    foreach (var oSalLine in oAdvanceDetail)
                                    {
                                        Hashtable elementGls = new Hashtable();
                                        var oEMPAdvance = (from a in oDBPrivate.TrnsAdvance
                                                           where a.ID == oSalLine.LineBaseEntry
                                                           select a).FirstOrDefault();
                                        elementGls = ds.getAdvGLClassified(oEmp, oEMPAdvance.MstAdvance, LocationID);
                                        TrnsSalaryClassification oAdvanceLine = new TrnsSalaryClassification();
                                        oDBPrivate.TrnsSalaryClassification.InsertOnSubmit(oAdvanceLine);
                                        oAdvanceLine.EmpID = oEmp.ID;
                                        oAdvanceLine.PeriodID = oPeriod.ID;
                                        oAdvanceLine.Location = LocationID;
                                        oAdvanceLine.DAccountCode = elementGls["DrAcct"].ToString();
                                        oAdvanceLine.DAccountDesc = elementGls["DrAcctName"].ToString();
                                        oAdvanceLine.CAccountCode = elementGls["CrAcct"].ToString(); ;
                                        oAdvanceLine.CAccountDesc = elementGls["CrAcctName"].ToString();
                                        //oAdvanceLine.LocationID = Convert.ToInt32(elementGls["LocationID"].ToString());
                                        oAdvanceLine.LineValue = oSalLine.LineValue * Oneline.FD;
                                    }

                                    #endregion

                                    #region Loan

                                    var oLoanDetail = (from a in oDBPrivate.TrnsSalaryProcessRegisterDetail
                                                       where a.SRID == oProcessSalary.Id
                                                       && a.LineType == "Loan Recovery"
                                                       select a).ToList();
                                    foreach (var oSalLine in oLoanDetail)
                                    {
                                        Hashtable elementGls = new Hashtable();
                                        var oEMPLoan = (from a in oDBPrivate.TrnsLoanDetail
                                                        where a.ID == oSalLine.LineBaseEntry
                                                        select a).FirstOrDefault();
                                        elementGls = ds.getLoanGLClassified(oEmp, oEMPLoan.MstLoans, LocationID);
                                        TrnsSalaryClassification oLoanLine = new TrnsSalaryClassification();
                                        oDBPrivate.TrnsSalaryClassification.InsertOnSubmit(oLoanLine);
                                        oLoanLine.EmpID = oEmp.ID;
                                        oLoanLine.PeriodID = oPeriod.ID;
                                        oLoanLine.Location = LocationID;
                                        oLoanLine.DAccountCode = elementGls["DrAcct"].ToString();
                                        oLoanLine.DAccountDesc = elementGls["DrAcctName"].ToString();
                                        oLoanLine.CAccountCode = elementGls["CrAcct"].ToString(); ;
                                        oLoanLine.CAccountDesc = elementGls["CrAcctName"].ToString();
                                        //oLoanLine.LocationID = Convert.ToInt32(elementGls["LocationID"].ToString());
                                        oLoanLine.LineValue = oSalLine.LineValue * Oneline.FD;
                                    }
                                    #endregion

                                    #region Tax

                                    var oTaxDetail = (from a in oDBPrivate.TrnsSalaryProcessRegisterDetail
                                                      where a.SRID == oProcessSalary.Id
                                                      && a.LineType == "Tax"
                                                      select a).FirstOrDefault();

                                    if (oTaxDetail != null)
                                    {
                                        TrnsSalaryClassification oTaxLine = new TrnsSalaryClassification();
                                        oDBPrivate.TrnsSalaryClassification.InsertOnSubmit(oTaxLine);
                                        oTaxLine.EmpID = oEmp.ID;
                                        oTaxLine.PeriodID = oPeriod.ID;
                                        oTaxLine.Location = LocationID;
                                        oTaxLine.DAccountCode = oGL.IncomeTaxExpense;
                                        oTaxLine.DAccountDesc = Program.objHrmsUI.getAcctName(oGL.IncomeTaxExpense);
                                        oTaxLine.CAccountCode = oGL.IncomeTaxPayable;
                                        oTaxLine.CAccountDesc = Program.objHrmsUI.getAcctName(oGL.IncomeTaxPayable);
                                        oTaxLine.LineValue = oTaxDetail.LineValue * Oneline.FD;
                                    }
                                    #endregion
                                }
                            }
                            oDBPrivate.SubmitChanges();
                        }
                    }
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

