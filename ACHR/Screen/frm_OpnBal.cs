using DIHRMS;
using DIHRMS.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Globalization;

namespace ACHR.Screen
{
    class frm_OpnBal : HRMSBaseForm
    {

        #region "Global Variables & Objects"

        SAPbouiCOM.Folder tbLoanBal, tbPFBal, tbGratBal, tbArerBal, tbLvsBal, tbConBal, tbSlryBal, tboTaxBal;
        SAPbouiCOM.Button btnImport, btnCancel, btnBrowse;
        SAPbouiCOM.Matrix mtLoanBal, mtPFBal, mtGratBal, mtArerBal, mtLevBal, mtContBal, mtSlryBal, mtTaxBal;
        SAPbouiCOM.Columns clmsLoan, clmsPFBal, clmsGratBal, clmsArerBal, clmsLevBal, clmsContBal, clmsSlryBal, clmsTaxBal;
        SAPbouiCOM.Column clmLoanNo, clmLoanEmpId, clmLoanLType, clmLoanDesc, clmLoanMonth, clmLoanOpening, clmLoanRecovered, clmLoanInstallment;
        SAPbouiCOM.Column clmPFBalNo, clmPFBalEmpId, clmPFBalEmpBal, clmPFBalEmployerBal, clmPFBalCumROI;
        SAPbouiCOM.Column clmGratNo, clmGratEmpId, clmGratOpnBal, clmGratMonth;
        SAPbouiCOM.Column clmArrearNo, clmArrearEmpId, clmArrearType, clmArrearDesc, clmArrearOpnBal, clmArrearMonth;
        SAPbouiCOM.Column clmLevNo, clmLevEmpID, clmLevType, clmLevDesc, clmLevUsed, clmLevCarryFrwrd, clmLevEntitled, clmLevFY;
        SAPbouiCOM.Column clmContNo, clmContEmpID, clmContType, clmContDesc, clmContBalance, clmContMonth;
        SAPbouiCOM.Column clmSlryNo, clmSlryEmpID, clmSlryOpeningBalance, clmSlryMonth;
        SAPbouiCOM.Column clmTBalNo, clmTBalEmpID, clmTBalOpeningBalance, clmTBalMonth;

        SAPbouiCOM.Item itbLoanBal, itbPFBal, itbGratBal, itbArerBal, itbLvsBal, itbConBal, itbSlryBal, itboTaxBal, ibtnImport, ibtnCancel, ibtnBrowse;
        SAPbouiCOM.DataTable dtLoan, dtPFBal, dtGratBal, dtArerBal, dtLevBal, dtContBal, dtSlryBal, dtTaxBal;

        String SelectTab = "tbLoanBa_1";
        DataTable DtFile = new DataTable();
        #endregion

        #region "Events"

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);

            if (pVal.Before_Action == false)
            {
                switch (pVal.ItemUID)
                {
                    case "btnImport":
                        ImportSelectedTabData();
                        break;
                    case "2":
                        break;
                    case "btnBrowse":
                        LoadDataInSelectedTab();
                        break;
                    case "tbLoanBa_1":
                    case "tbPFBal_2":
                    case "tbGratBa_3":
                    case "tbArerBa_4":
                    case "tbConBal_6":
                    case "tbLvsBal_5":
                    case "tbSlryBa_7":
                    case "tboTaxBa_8":
                        SelectTab = pVal.ItemUID;
                        break;
                    default:
                        break;
                }
            }

        }

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);

            InitiallizeForm();
        }

        #endregion

        #region "Local Methods"

        private void InitiallizeForm()
        {
            //Each Item should be initiallized in this sequence

            /*
             * Add datasource for the item
             * Initiallize the control object with same ID
             * Initiallize the Item Object with same ID
             * Data Bind the controll with data source
             * 
             * */
            try
            {
                oForm.Freeze(true);

                tbLoanBal = oForm.Items.Item("tbLoanBa_1").Specific;
                itbLoanBal = oForm.Items.Item("tbLoanBa_1");

                tbPFBal = oForm.Items.Item("tbPFBal_2").Specific;
                itbPFBal = oForm.Items.Item("tbPFBal_2");

                tbGratBal = oForm.Items.Item("tbGratBa_3").Specific;
                itbGratBal = oForm.Items.Item("tbGratBa_3");

                //tbArerBal = oForm.Items.Item("tbArerBa_4").Specific;
                //itbArerBal = oForm.Items.Item("tbArerBa_4");

                tbLvsBal = oForm.Items.Item("tbLvsBal_5").Specific;
                itbLvsBal = oForm.Items.Item("tbLvsBal_5");

                tbConBal = oForm.Items.Item("tbConBal_6").Specific;
                itbConBal = oForm.Items.Item("tbConBal_6");

                tbSlryBal = oForm.Items.Item("tbSlryBa_7").Specific;
                itbSlryBal = oForm.Items.Item("tbSlryBa_7");

                tboTaxBal = oForm.Items.Item("tboTaxBa_8").Specific;
                itboTaxBal = oForm.Items.Item("tboTaxBa_8");

                btnImport = oForm.Items.Item("btnImport").Specific;
                ibtnImport = oForm.Items.Item("btnImport");

                btnCancel = oForm.Items.Item("2").Specific;
                ibtnCancel = oForm.Items.Item("2");

                btnBrowse = oForm.Items.Item("btnBrowse").Specific;
                ibtnBrowse = oForm.Items.Item("btnBrowse");

                mtLoanBal = oForm.Items.Item("mtLoanBal").Specific;
                dtLoan = oForm.DataSources.DataTables.Add("LoanBalance");
                dtLoan.Columns.Add("clNo", SAPbouiCOM.BoFieldsType.ft_Integer, 5);
                dtLoan.Columns.Add("clEmpID", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 20);
                dtLoan.Columns.Add("clLoanType", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 10);
                dtLoan.Columns.Add("clDesc", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 100);
                dtLoan.Columns.Add("clMonth", SAPbouiCOM.BoFieldsType.ft_Text, 10);
                dtLoan.Columns.Add("clOpnLoan", SAPbouiCOM.BoFieldsType.ft_Sum);
                dtLoan.Columns.Add("clRecvry", SAPbouiCOM.BoFieldsType.ft_Sum);
                dtLoan.Columns.Add("clInstlmnt", SAPbouiCOM.BoFieldsType.ft_Sum);
                dtLoan.Columns.Add("clActive", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 10);
                dtLoan.Rows.Clear();

                clmsLoan = (SAPbouiCOM.Columns)mtLoanBal.Columns;
                clmLoanNo = clmsLoan.Item("clNo");
                clmLoanNo.DataBind.Bind("LoanBalance", "clNo");
                clmLoanEmpId = clmsLoan.Item("clEmpID");
                clmLoanEmpId.DataBind.Bind("LoanBalance", "clEmpID");
                clmLoanLType = clmsLoan.Item("clLoanType");
                clmLoanLType.DataBind.Bind("LoanBalance", "clLoanType");
                clmLoanDesc = clmsLoan.Item("clDesc");
                clmLoanDesc.DataBind.Bind("LoanBalance", "clDesc");
                clmLoanMonth = clmsLoan.Item("clMonth");
                clmLoanMonth.DataBind.Bind("LoanBalance", "clMonth");
                clmLoanOpening = clmsLoan.Item("clOpnLoan");
                clmLoanOpening.DataBind.Bind("LoanBalance", "clOpnLoan");
                clmLoanRecovered = clmsLoan.Item("clRecvry");
                clmLoanRecovered.DataBind.Bind("LoanBalance", "clRecvry");
                clmLoanInstallment = clmsLoan.Item("clInstlmnt");
                clmLoanInstallment.DataBind.Bind("LoanBalance", "clInstlmnt");

                mtPFBal = oForm.Items.Item("mtPFBal").Specific;
                dtPFBal = oForm.DataSources.DataTables.Add("PFBalance");
                dtPFBal.Columns.Add("clNo", SAPbouiCOM.BoFieldsType.ft_Integer, 5);
                dtPFBal.Columns.Add("clEmpID", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 20);
                dtPFBal.Columns.Add("clEmpBal", SAPbouiCOM.BoFieldsType.ft_Sum);
                dtPFBal.Columns.Add("clEmprBal", SAPbouiCOM.BoFieldsType.ft_Sum);
                dtPFBal.Columns.Add("clROI", SAPbouiCOM.BoFieldsType.ft_Rate);
                dtPFBal.Rows.Clear();

                clmsPFBal = (SAPbouiCOM.Columns)mtPFBal.Columns;
                clmPFBalNo = clmsPFBal.Item("clNo");
                clmPFBalNo.DataBind.Bind("PFBalance", "clNo");
                clmPFBalEmpId = clmsPFBal.Item("clEmpID");
                clmPFBalEmpId.DataBind.Bind("PFBalance", "clEmpID");
                clmPFBalEmpBal = clmsPFBal.Item("clEmpBal");
                clmPFBalEmpBal.DataBind.Bind("PFBalance", "clEmpBal");
                clmPFBalEmployerBal = clmsPFBal.Item("clEmprBal");
                clmPFBalEmployerBal.DataBind.Bind("PFBalance", "clEmprBal");
                clmPFBalCumROI = clmsPFBal.Item("clROI");
                clmPFBalCumROI.DataBind.Bind("PFBalance", "clROI");

                mtLevBal = oForm.Items.Item("mtLevBal").Specific;
                dtLevBal = oForm.DataSources.DataTables.Add("LeaveBalance");

                dtLevBal.Columns.Add("clNo", SAPbouiCOM.BoFieldsType.ft_Integer, 5);
                dtLevBal.Columns.Add("clEmpID", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 20);
                dtLevBal.Columns.Add("clLevType", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 10);
                dtLevBal.Columns.Add("clDesc", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 100);
                dtLevBal.Columns.Add("clLeVusd", SAPbouiCOM.BoFieldsType.ft_Quantity, 10);
                dtLevBal.Columns.Add("clLevEnt", SAPbouiCOM.BoFieldsType.ft_Quantity, 10);
                dtLevBal.Columns.Add("clCarFr", SAPbouiCOM.BoFieldsType.ft_Quantity, 10);
                dtLevBal.Columns.Add("clFSY", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 50);

                dtLevBal.Rows.Clear();

                clmsLevBal = (SAPbouiCOM.Columns)mtLevBal.Columns;
                clmLevNo = clmsLevBal.Item("clNo");
                clmLevNo.DataBind.Bind("LeaveBalance", "clNo");
                clmLevEmpID = clmsLevBal.Item("clEmpID");
                clmLevEmpID.DataBind.Bind("LeaveBalance", "clEmpID");
                clmLevType = clmsLevBal.Item("clLevType");
                clmLevType.DataBind.Bind("LeaveBalance", "clLevType");
                clmLevDesc = clmsLevBal.Item("clDesc");
                clmLevDesc.DataBind.Bind("LeaveBalance", "clDesc");

                clmLevUsed = clmsLevBal.Item("clLeVusd");
                clmLevUsed.DataBind.Bind("LeaveBalance", "clLeVusd");
                clmLevEntitled = clmsLevBal.Item("clLevEnt");
                clmLevEntitled.DataBind.Bind("LeaveBalance", "clLevEnt");
                clmLevCarryFrwrd = clmsLevBal.Item("clCarFr");
                clmLevCarryFrwrd.DataBind.Bind("LeaveBalance", "clCarFr");
                clmLevFY = clmsLevBal.Item("clFSY");
                clmLevFY.DataBind.Bind("LeaveBalance", "clFSY");



                mtGratBal = oForm.Items.Item("mtGratBal").Specific;
                dtGratBal = oForm.DataSources.DataTables.Add("Gratuity");
                dtGratBal.Columns.Add("clNo", SAPbouiCOM.BoFieldsType.ft_Integer, 5);
                dtGratBal.Columns.Add("clEmpID", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 20);
                dtGratBal.Columns.Add("clOpnBal", SAPbouiCOM.BoFieldsType.ft_Sum);
                dtGratBal.Columns.Add("clMonth", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 10);
                dtGratBal.Rows.Clear();

                clmsGratBal = (SAPbouiCOM.Columns)mtGratBal.Columns;
                clmGratNo = clmsGratBal.Item("clNo");
                clmGratNo.DataBind.Bind("Gratuity", "clNo");
                clmGratEmpId = clmsGratBal.Item("clEmpID");
                clmGratEmpId.DataBind.Bind("Gratuity", "clEmpID");
                clmGratOpnBal = clmsGratBal.Item("clOpnBal");
                clmGratOpnBal.DataBind.Bind("Gratuity", "clOpnBal");
                clmGratMonth = clmsGratBal.Item("clMonth");
                clmGratMonth.DataBind.Bind("Gratuity", "clMonth");

                mtContBal = oForm.Items.Item("mtContBal").Specific;
                dtContBal = oForm.DataSources.DataTables.Add("Contribution");
                dtContBal.Columns.Add("clNo", SAPbouiCOM.BoFieldsType.ft_Integer, 5);
                dtContBal.Columns.Add("clEmpID", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 20);
                dtContBal.Columns.Add("clConType", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 10);
                dtContBal.Columns.Add("clDesc", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 10);
                dtContBal.Columns.Add("clOpnBal", SAPbouiCOM.BoFieldsType.ft_Quantity, 10);
                dtContBal.Columns.Add("clMonth", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 10);
                dtContBal.Rows.Clear();

                clmsContBal = (SAPbouiCOM.Columns)mtContBal.Columns;
                clmContNo = clmsContBal.Item("clNo");
                clmContNo.DataBind.Bind("Contribution", "clNo");
                clmContEmpID = clmsContBal.Item("clEmpID");
                clmContEmpID.DataBind.Bind("Contribution", "clEmpID");
                clmContType = clmsContBal.Item("clConType");
                clmContType.DataBind.Bind("Contribution", "clConType");
                clmContDesc = clmsContBal.Item("clDesc");
                clmContDesc.DataBind.Bind("Contribution", "clDesc");
                clmContBalance = clmsContBal.Item("clOpnBal");
                clmContBalance.DataBind.Bind("Contribution", "clOpnBal");
                clmContMonth = clmsContBal.Item("clMonth");
                clmContMonth.DataBind.Bind("Contribution", "clMonth");

                //mtArerBal = oForm.Items.Item("mtArerBal").Specific;
                //dtArerBal = oForm.DataSources.DataTables.Add("Arrears");
                //dtArerBal.Columns.Add("clNo", SAPbouiCOM.BoFieldsType.ft_Integer, 5);
                //dtArerBal.Columns.Add("clEmpID", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 20);
                //dtArerBal.Columns.Add("clArrear", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 10);
                //dtArerBal.Columns.Add("clDesc", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 100);
                //dtArerBal.Columns.Add("clOpnBal", SAPbouiCOM.BoFieldsType.ft_Quantity, 10);
                //dtArerBal.Columns.Add("clMonth", SAPbouiCOM.BoFieldsType.ft_Date, 10);

                //clmsArerBal = (SAPbouiCOM.Columns)mtArerBal.Columns;
                //clmArrearNo = clmsArerBal.Item("clNo");
                //clmArrearNo.DataBind.Bind("Arrears", "clNo");
                //clmArrearEmpId = clmsArerBal.Item("clEmpID");
                //clmArrearEmpId.DataBind.Bind("Arrears", "clEmpID");
                //clmArrearType = clmsArerBal.Item("clArrear");
                //clmArrearType.DataBind.Bind("Arrears", "clArrear");
                //clmArrearDesc = clmsArerBal.Item("clDesc");
                //clmArrearDesc.DataBind.Bind("Arrears", "clDesc");
                //clmArrearOpnBal = clmsArerBal.Item("clOpnBal");
                //clmArrearOpnBal.DataBind.Bind("Arrears", "clOpnBal");
                //clmArrearMonth = clmsArerBal.Item("clMonth");
                //clmArrearMonth.DataBind.Bind("Arrears", "clMonth");

                mtSlryBal = oForm.Items.Item("mtSlryBal").Specific;
                dtSlryBal = oForm.DataSources.DataTables.Add("Salary");
                dtSlryBal.Columns.Add("clNo", SAPbouiCOM.BoFieldsType.ft_Integer, 5);
                dtSlryBal.Columns.Add("clEmpID", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 20);
                dtSlryBal.Columns.Add("clOpnBal", SAPbouiCOM.BoFieldsType.ft_Sum);
                dtSlryBal.Columns.Add("clMonth", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 10);

                clmsSlryBal = (SAPbouiCOM.Columns)mtSlryBal.Columns;
                clmSlryNo = clmsSlryBal.Item("clNo");
                clmSlryNo.DataBind.Bind("Salary", "clNo");
                clmSlryEmpID = clmsSlryBal.Item("clEmpID");
                clmSlryEmpID.DataBind.Bind("Salary", "clEmpID");
                clmSlryOpeningBalance = clmsSlryBal.Item("clOpnBal");
                clmSlryOpeningBalance.DataBind.Bind("Salary", "clOpnBal");
                clmSlryMonth = clmsSlryBal.Item("clMonth");
                clmSlryMonth.DataBind.Bind("Salary", "clMonth");

                mtTaxBal = oForm.Items.Item("mtTaxBal").Specific;
                dtTaxBal = oForm.DataSources.DataTables.Add("Taxed");
                dtTaxBal.Columns.Add("clNo", SAPbouiCOM.BoFieldsType.ft_Integer, 5);
                dtTaxBal.Columns.Add("clEmpID", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 20);
                dtTaxBal.Columns.Add("clOpnBal", SAPbouiCOM.BoFieldsType.ft_Sum);
                dtTaxBal.Columns.Add("clMonth", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 10);

                clmsTaxBal = (SAPbouiCOM.Columns)mtTaxBal.Columns;
                clmTBalNo = clmsTaxBal.Item("clNo");
                clmTBalNo.DataBind.Bind("Taxed", "clNo");
                clmTBalEmpID = clmsTaxBal.Item("clEmpID");
                clmTBalEmpID.DataBind.Bind("Taxed", "clEmpID");
                clmTBalOpeningBalance = clmsTaxBal.Item("clOpnBal");
                clmTBalOpeningBalance.DataBind.Bind("Taxed", "clOpnBal");
                clmTBalMonth = clmsTaxBal.Item("clMonth");
                clmTBalMonth.DataBind.Bind("Taxed", "clMonth");

                oForm.PaneLevel = 1;

                oForm.Freeze(false);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            finally
            {
                oForm.Freeze(false);
            }
        }

        private void ImportSelectedTabData()
        {
            try
            {
                switch (SelectTab)
                {
                    case "tbLoanBa_1":
                        LoanBalanceImport();
                        break;
                    case "tbPFBal_2":
                        PFBalanceImport();
                        break;
                    case "tbGratBa_3":
                        GratuityImport();
                        break;
                    case "tbArerBa_4":
                        ArrearImport();
                        break;
                    case "tbConBal_6":
                        ContributionImport();
                        break;
                    case "tbLvsBal_5":
                        if (Program.systemInfo.FlgLeaveCalendar == true)
                        {
                            LeaveBalanceImportFromLeaveCalendar();
                        }
                        else
                        {
                            LeaveBalanceImport();
                        }
                        break;
                    case "tbSlryBa_7":
                        SalaryImport();
                        break;
                    case "tboTaxBa_8":
                        TaxBalanceImport();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void LoadDataInSelectedTab()
        {
            try
            {
                switch (SelectTab)
                {
                    case "tbLoanBa_1":
                        LoanBalanceToGrid();
                        break;
                    case "tbPFBal_2":
                        PFBalanceToGrid();
                        break;
                    case "tbGratBa_3":
                        GratuityToGrid();
                        break;
                    case "tbArerBa_4":
                        ArrearToGrid();
                        break;
                    case "tbConBal_6":
                        ContributionToGrid();
                        break;
                    case "tbLvsBal_5":
                        LeavesBalanceToGrid();
                        break;
                    case "tbSlryBa_7":
                        SalaryToGrid();
                        break;
                    case "tboTaxBa_8":
                        TaxBalanceToGrid();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void LoanBalanceToGrid()
        {
            String FilePath, OneLine;
            String[] OneLineParsed = new String[8];
            Int16 counter = 0;
            try
            {
                DtFile.Columns.Clear();
                DtFile.Columns.Add("SrNo");
                DtFile.Columns.Add("EmpID");
                DtFile.Columns.Add("LoanType");
                DtFile.Columns.Add("Description");
                DtFile.Columns.Add("Month");
                DtFile.Columns.Add("LoanAmount");
                DtFile.Columns.Add("RecoveredAmount");
                DtFile.Columns.Add("Installment");
                DtFile.Columns.Add("Active");
                FilePath = Program.objHrmsUI.FindFile();

                if (!String.IsNullOrEmpty(FilePath))
                {
                    using (StreamReader File = new StreamReader(FilePath))
                    {
                        File.ReadLine();
                        DtFile.Rows.Clear();

                        while (true)
                        {
                            OneLine = File.ReadLine();
                            if (String.IsNullOrEmpty(OneLine))
                            {
                                break;
                            }
                            else
                            {
                                OneLineParsed = OneLine.Split(',');
                                DtFile.Rows.Add(counter, OneLineParsed[0], OneLineParsed[1], OneLineParsed[2], OneLineParsed[3], OneLineParsed[4], OneLineParsed[5], OneLineParsed[6], OneLineParsed[7]);
                                counter++;
                            }
                        }
                    }
                }

                if (DtFile.Rows.Count > 0)
                {
                    Int16 LineNumber = 1;
                    mtLoanBal.Clear();
                    dtLoan.Rows.Clear();
                    foreach (DataRow dr in DtFile.Rows)
                    {
                        string datetime = dr["Month"].ToString();

                        dtLoan.Rows.Add();
                        dtLoan.SetValue("clNo", LineNumber - 1, LineNumber);
                        dtLoan.SetValue("clEmpID", LineNumber - 1, dr["EmpID"]);
                        dtLoan.SetValue("clLoanType", LineNumber - 1, dr["LoanType"]);
                        dtLoan.SetValue("clDesc", LineNumber - 1, dr["Description"]);
                        dtLoan.SetValue("clMonth", LineNumber - 1, datetime);
                        dtLoan.SetValue("clOpnLoan", LineNumber - 1, dr["LoanAmount"]);
                        dtLoan.SetValue("clRecvry", LineNumber - 1, dr["RecoveredAmount"]);
                        dtLoan.SetValue("clInstlmnt", LineNumber - 1, dr["Installment"]);
                        //dtLoan.SetValue("clActive", LineNumber - 1, dr["Active"]);
                        dtLoan.SetValue("clActive", LineNumber - 1, "Y");
                        LineNumber++;
                    }
                    mtLoanBal.LoadFromDataSource();


                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("LoanBalanceToGrid : " + Ex.Message + counter, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        //private void LoanBalanceToGrid()
        //{
        //    String FilePath, OneLine;
        //    String[] OneLineParsed = new String[8];
        //    Int16 counter = 0;
        //    try
        //    {
        //        DtFile.Columns.Clear();
        //        DtFile.Columns.Add("SrNo");
        //        DtFile.Columns.Add("EmpID");
        //        DtFile.Columns.Add("LoanType");
        //        DtFile.Columns.Add("Description");
        //        DtFile.Columns.Add("Month");
        //        DtFile.Columns.Add("LoanAmount");
        //        DtFile.Columns.Add("RecoveredAmount");
        //        DtFile.Columns.Add("Installment");
        //        DtFile.Columns.Add("Active");
        //        FilePath = Program.objHrmsUI.FindFile();

        //        if (!String.IsNullOrEmpty(FilePath))
        //        {
        //            using (StreamReader File = new StreamReader(FilePath))
        //            {
        //                File.ReadLine();
        //                DtFile.Rows.Clear();

        //                while (true)
        //                {
        //                    OneLine = File.ReadLine();
        //                    if (String.IsNullOrEmpty(OneLine))
        //                    {
        //                        break;
        //                    }
        //                    else
        //                    {
        //                        OneLineParsed = OneLine.Split(',');
        //                        DtFile.Rows.Add(counter, OneLineParsed[0], OneLineParsed[1], OneLineParsed[2], OneLineParsed[3], OneLineParsed[4], OneLineParsed[5], OneLineParsed[6], OneLineParsed[7]);
        //                        counter++;
        //                    }
        //                }
        //            }
        //        }

        //        if (DtFile.Rows.Count > 0)
        //        {
        //            Int16 LineNumber = 1;
        //            mtLoanBal.Clear();
        //            dtLoan.Rows.Clear();
        //            foreach (DataRow dr in DtFile.Rows)
        //            {
        //                string datetime = dr["Month"].ToString();

        //                dtLoan.Rows.Add();
        //                dtLoan.SetValue("clNo", LineNumber - 1, LineNumber);
        //                dtLoan.SetValue("clEmpID", LineNumber - 1, dr["EmpID"]);
        //                dtLoan.SetValue("clLoanType", LineNumber - 1, dr["LoanType"]);
        //                dtLoan.SetValue("clDesc", LineNumber - 1, dr["Description"]);
        //                dtLoan.SetValue("clMonth", LineNumber - 1, datetime);
        //                dtLoan.SetValue("clOpnLoan", LineNumber - 1, dr["LoanAmount"]);
        //                dtLoan.SetValue("clRecvry", LineNumber - 1, dr["RecoveredAmount"]);
        //                dtLoan.SetValue("clInstlmnt", LineNumber - 1, dr["Installment"]);
        //                //dtLoan.SetValue("clActive", LineNumber - 1, dr["Active"]);
        //                dtLoan.SetValue("clActive", LineNumber - 1, "Y");
        //                LineNumber++;
        //            }
        //            mtLoanBal.LoadFromDataSource();


        //        }
        //    }
        //    catch (Exception Ex)
        //    {
        //        oApplication.StatusBar.SetText("LoanBalanceToGrid : " + Ex.Message + counter, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
        //    }
        //}

        private void PFBalanceToGrid()
        {
            String FilePath, OneLine;
            String[] OneLineParsed = new String[4];

            try
            {
                DtFile.Columns.Clear();
                DtFile.Columns.Add("SrNo");
                DtFile.Columns.Add("EmpID");
                DtFile.Columns.Add("EmployeeBalance");
                DtFile.Columns.Add("EmployerBalance");
                DtFile.Columns.Add("CummulativeROI");
                FilePath = Program.objHrmsUI.FindFile();
                if (!String.IsNullOrEmpty(FilePath))
                {
                    using (StreamReader File = new StreamReader(FilePath))
                    {
                        File.ReadLine();
                        DtFile.Rows.Clear();
                        Int16 counter = 1;
                        while (true)
                        {
                            OneLine = File.ReadLine();
                            if (String.IsNullOrEmpty(OneLine))
                            {
                                break;
                            }
                            else
                            {
                                OneLineParsed = OneLine.Split(',');
                                DtFile.Rows.Add(counter, OneLineParsed[0], OneLineParsed[1], OneLineParsed[2], OneLineParsed[3]);
                                counter++;
                            }
                        }
                    }
                }

                if (DtFile.Rows.Count > 0)
                {
                    mtPFBal.Clear();
                    dtPFBal.Rows.Clear();
                    int LineNumber = 1;
                    foreach (DataRow dr in DtFile.Rows)
                    {
                        dtPFBal.Rows.Add();

                        dtPFBal.SetValue("clNo", LineNumber - 1, LineNumber);
                        dtPFBal.SetValue("clEmpID", LineNumber - 1, dr["EmpID"]);
                        dtPFBal.SetValue("clEmpBal", LineNumber - 1, dr["EmployeeBalance"]);
                        dtPFBal.SetValue("clEmprBal", LineNumber - 1, dr["EmployerBalance"]);
                        dtPFBal.SetValue("clROI", LineNumber - 1, dr["CummulativeROI"]);

                        LineNumber++;
                    }
                    mtPFBal.LoadFromDataSource();


                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("PFBalanceToGrid : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void GratuityToGrid()
        {
            String FilePath, OneLine;
            String[] OneLineParsed = new String[4];

            try
            {
                DtFile.Columns.Clear();
                DtFile.Columns.Add("SrNo");
                DtFile.Columns.Add("EmpID");
                DtFile.Columns.Add("OpeningBalance");
                DtFile.Columns.Add("Month");

                FilePath = Program.objHrmsUI.FindFile();
                if (!String.IsNullOrEmpty(FilePath))
                {
                    using (StreamReader File = new StreamReader(FilePath))
                    {
                        File.ReadLine();
                        DtFile.Rows.Clear();
                        Int16 counter = 1;
                        while (true)
                        {
                            OneLine = File.ReadLine();
                            if (String.IsNullOrEmpty(OneLine))
                            {
                                break;
                            }
                            else
                            {
                                OneLineParsed = OneLine.Split(',');
                                DtFile.Rows.Add(counter, OneLineParsed[0], OneLineParsed[1], OneLineParsed[2]);
                                counter++;
                            }
                        }
                    }
                }

                if (DtFile.Rows.Count > 0)
                {
                    Int16 LineNumber = 1;
                    mtGratBal.Clear();
                    dtGratBal.Rows.Clear();
                    foreach (DataRow dr in DtFile.Rows)
                    {
                        dtGratBal.Rows.Add();
                        dtGratBal.SetValue("clNo", LineNumber - 1, LineNumber);
                        dtGratBal.SetValue("clEmpID", LineNumber - 1, dr["EmpID"]);
                        dtGratBal.SetValue("clOpnBal", LineNumber - 1, dr["OpeningBalance"]);
                        dtGratBal.SetValue("clMonth", LineNumber - 1, Convert.ToDateTime(dr["Month"].ToString()));

                        LineNumber++;
                    }
                    mtGratBal.LoadFromDataSource();


                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("GratuityBalanceToGrid : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void LeavesBalanceToGrid()
        {
            String FilePath, OneLine;
            String[] OneLineParsed = new String[7];

            try
            {
                DtFile.Columns.Clear();
                DtFile.Columns.Add("SrNo");
                DtFile.Columns.Add("EmpID");
                DtFile.Columns.Add("LeaveType");
                DtFile.Columns.Add("Description");
                DtFile.Columns.Add("LeavesUsed");
                DtFile.Columns.Add("LeavesEntitled");
                DtFile.Columns.Add("LeavesCarryForward");
                DtFile.Columns.Add("FiscalYear");
                DtFile.Columns.Add("Month");

                FilePath = Program.objHrmsUI.FindFile();
                if (!String.IsNullOrEmpty(FilePath))
                {
                    using (StreamReader File = new StreamReader(FilePath))
                    {
                        File.ReadLine();
                        DtFile.Rows.Clear();
                        Int16 counter = 1;
                        while (true)
                        {
                            OneLine = File.ReadLine();
                            if (String.IsNullOrEmpty(OneLine))
                            {
                                break;
                            }
                            else
                            {
                                OneLineParsed = OneLine.Split(',');
                                DtFile.Rows.Add(counter, OneLineParsed[0], OneLineParsed[1], OneLineParsed[2], OneLineParsed[3], OneLineParsed[4], OneLineParsed[5], OneLineParsed[6], OneLineParsed[7]);
                                counter++;
                            }
                        }
                    }
                }

                if (DtFile.Rows.Count > 0)
                {
                    Int16 LineNumber = 1;
                    mtLevBal.Clear();
                    dtLevBal.Rows.Clear();
                    foreach (DataRow dr in DtFile.Rows)
                    {
                        string desc = (from a in dbHrPayroll.MstLeaveType where a.Code.Contains(dr["LeaveType"].ToString()) select a.Description).FirstOrDefault();
                        dtLevBal.Rows.Add();
                        dtLevBal.SetValue("clNo", LineNumber - 1, LineNumber);
                        dtLevBal.SetValue("clEmpID", LineNumber - 1, dr["EmpID"]);
                        dtLevBal.SetValue("clLevType", LineNumber - 1, dr["LeaveType"]);
                        dtLevBal.SetValue("clDesc", LineNumber - 1, desc);
                        //string DtTime = dr["Month"].ToString();
                        // DateTime dtNow = Convert.ToDateTime(DtTime);
                        //dtLevBal.SetValue("clMonth", LineNumber - 1, Convert.ToDateTime(dr["Month"].ToString()));
                        dtLevBal.SetValue("clLeVusd", LineNumber - 1, dr["LeavesUsed"]);
                        dtLevBal.SetValue("clLevEnt", LineNumber - 1, dr["LeavesEntitled"]);
                        dtLevBal.SetValue("clCarFr", LineNumber - 1, dr["LeavesCarryForward"]);
                        dtLevBal.SetValue("clFSY", LineNumber - 1, dr["FiscalYear"]);

                        LineNumber++;
                    }
                    mtLevBal.LoadFromDataSource();


                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("LeavesBalanceToGrid : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void ArrearToGrid()
        {
            String FilePath, OneLine;
            String[] OneLineParsed = new String[6];

            try
            {
                DtFile.Columns.Clear();
                DtFile.Columns.Add("SrNo");
                DtFile.Columns.Add("EmpID");
                DtFile.Columns.Add("ArrearType");
                DtFile.Columns.Add("Description");
                DtFile.Columns.Add("OpeningBalance");
                DtFile.Columns.Add("Month");

                FilePath = Program.objHrmsUI.FindFile();
                if (!String.IsNullOrEmpty(FilePath))
                {
                    using (StreamReader File = new StreamReader(FilePath))
                    {
                        File.ReadLine();
                        DtFile.Rows.Clear();
                        Int16 counter = 1;
                        while (true)
                        {
                            OneLine = File.ReadLine();
                            if (String.IsNullOrEmpty(OneLine))
                            {
                                break;
                            }
                            else
                            {
                                OneLineParsed = OneLine.Split(',');
                                DtFile.Rows.Add(counter, OneLineParsed[0], OneLineParsed[1], OneLineParsed[2], OneLineParsed[3], OneLineParsed[4]);
                                counter++;
                            }
                        }
                    }
                }

                if (DtFile.Rows.Count > 0)
                {
                    Int16 LineNumber = 1;
                    mtArerBal.Clear();
                    dtArerBal.Rows.Clear();
                    foreach (DataRow dr in DtFile.Rows)
                    {
                        dtArerBal.Rows.Add();
                        dtArerBal.SetValue("clNo", LineNumber - 1, LineNumber);
                        dtArerBal.SetValue("clEmpID", LineNumber - 1, dr["EmpID"]);
                        dtArerBal.SetValue("clArrear", LineNumber - 1, dr["ArrearType"]);
                        dtArerBal.SetValue("clDesc", LineNumber - 1, dr["Description"]);
                        dtArerBal.SetValue("clOpnBal", LineNumber - 1, dr["OpeningBalance"]);
                        dtArerBal.SetValue("clMonth", LineNumber - 1, Convert.ToDateTime(dr["Month"].ToString()));

                        LineNumber++;
                    }
                    mtArerBal.LoadFromDataSource();


                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("ArrearToGrid : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void ContributionToGrid()
        {
            String FilePath, OneLine;
            String[] OneLineParsed = new String[6];

            try
            {
                DtFile.Columns.Clear();
                DtFile.Columns.Add("SrNo");
                DtFile.Columns.Add("EmpID");
                DtFile.Columns.Add("ContributionType");
                DtFile.Columns.Add("Description");
                DtFile.Columns.Add("OpeningBalance");
                DtFile.Columns.Add("Month");

                FilePath = Program.objHrmsUI.FindFile();
                if (!String.IsNullOrEmpty(FilePath))
                {
                    using (StreamReader File = new StreamReader(FilePath))
                    {
                        File.ReadLine();
                        DtFile.Rows.Clear();
                        Int16 counter = 1;
                        while (true)
                        {
                            OneLine = File.ReadLine();
                            if (String.IsNullOrEmpty(OneLine))
                            {
                                break;
                            }
                            else
                            {
                                OneLineParsed = OneLine.Split(',');
                                DtFile.Rows.Add(counter, OneLineParsed[0], OneLineParsed[1], OneLineParsed[2], OneLineParsed[3], OneLineParsed[4]);
                                counter++;
                            }
                        }
                    }
                }

                if (DtFile.Rows.Count > 0)
                {
                    Int16 LineNumber = 1;
                    mtContBal.Clear();
                    dtContBal.Rows.Clear();
                    foreach (DataRow dr in DtFile.Rows)
                    {
                        dtContBal.Rows.Add();
                        dtContBal.SetValue("clNo", LineNumber - 1, LineNumber);
                        dtContBal.SetValue("clEmpID", LineNumber - 1, dr["EmpID"]);
                        dtContBal.SetValue("clConType", LineNumber - 1, dr["ContributionType"]);
                        dtContBal.SetValue("clDesc", LineNumber - 1, dr["Description"]);
                        dtContBal.SetValue("clOpnBal", LineNumber - 1, dr["OpeningBalance"]);
                        dtContBal.SetValue("clMonth", LineNumber - 1, dr["Month"].ToString());

                        LineNumber++;
                    }
                    mtContBal.LoadFromDataSource();


                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("ContributionToGrid : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void SalaryToGrid()
        {
            String FilePath, OneLine;
            String[] OneLineParsed = new String[3];

            try
            {
                DtFile.Columns.Clear();
                DtFile.Columns.Add("SrNo");
                DtFile.Columns.Add("EmpID");
                DtFile.Columns.Add("OpeningBalance");
                DtFile.Columns.Add("Month");

                FilePath = Program.objHrmsUI.FindFile();
                if (!String.IsNullOrEmpty(FilePath))
                {
                    using (StreamReader File = new StreamReader(FilePath))
                    {
                        File.ReadLine();
                        DtFile.Rows.Clear();
                        Int16 counter = 1;
                        while (true)
                        {
                            OneLine = File.ReadLine();
                            if (String.IsNullOrEmpty(OneLine))
                            {
                                break;
                            }
                            else
                            {
                                OneLineParsed = OneLine.Split(',');
                                DtFile.Rows.Add(counter, OneLineParsed[0], OneLineParsed[1], OneLineParsed[2]);
                                counter++;
                            }
                        }
                    }
                }

                if (DtFile.Rows.Count > 0)
                {
                    Int16 LineNumber = 1;
                    mtSlryBal.Clear();
                    dtSlryBal.Rows.Clear();
                    foreach (DataRow dr in DtFile.Rows)
                    {
                        dtSlryBal.Rows.Add();
                        dtSlryBal.SetValue("clNo", LineNumber - 1, LineNumber);
                        dtSlryBal.SetValue("clEmpID", LineNumber - 1, dr["EmpID"]);
                        dtSlryBal.SetValue("clOpnBal", LineNumber - 1, dr["OpeningBalance"]);
                        dtSlryBal.SetValue("clMonth", LineNumber - 1, dr["Month"]);

                        LineNumber++;
                    }
                    mtSlryBal.LoadFromDataSource();

                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("SalaryToGrid : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void TaxBalanceToGrid()
        {
            String FilePath, OneLine;
            String[] OneLineParsed = new String[4];

            try
            {
                DtFile.Columns.Clear();
                DtFile.Columns.Add("SrNo");
                DtFile.Columns.Add("EmpID");
                DtFile.Columns.Add("OpeningBalance");
                DtFile.Columns.Add("Month");

                FilePath = Program.objHrmsUI.FindFile();
                if (!String.IsNullOrEmpty(FilePath))
                {
                    using (StreamReader File = new StreamReader(FilePath))
                    {
                        File.ReadLine();
                        DtFile.Rows.Clear();
                        Int16 counter = 1;
                        while (true)
                        {
                            OneLine = File.ReadLine();
                            if (String.IsNullOrEmpty(OneLine))
                            {
                                break;
                            }
                            else
                            {
                                OneLineParsed = OneLine.Split(',');
                                DtFile.Rows.Add(counter, OneLineParsed[0], OneLineParsed[1], OneLineParsed[2]);
                                counter++;
                            }
                        }
                    }
                }

                if (DtFile.Rows.Count > 0)
                {
                    Int16 LineNumber = 1;
                    mtTaxBal.Clear();
                    dtTaxBal.Rows.Clear();
                    foreach (DataRow dr in DtFile.Rows)
                    {
                        dtTaxBal.Rows.Add();
                        dtTaxBal.SetValue("clNo", LineNumber - 1, LineNumber);
                        dtTaxBal.SetValue("clEmpID", LineNumber - 1, dr["EmpID"]);
                        dtTaxBal.SetValue("clOpnBal", LineNumber - 1, dr["OpeningBalance"]);
                        dtTaxBal.SetValue("clMonth", LineNumber - 1, dr["Month"].ToString());

                        LineNumber++;
                    }
                    mtTaxBal.LoadFromDataSource();

                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("TaxBalanceToGrid : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void LoanBalanceImport()
        {
            string strDocStatus = "LV0003", strApprovalStatus = "LV0006";
            decimal LoanAmount = 0, RecoveredAmount = 0, DueAmount = 0;
            //Variable Section

            //Logic
            try
            {
                foreach (DataRow dr in DtFile.Rows)
                {
                    int LoanIDCount = 0, EmpIDCount = 0;
                    LoanIDCount = (from a in dbHrPayroll.MstLoans
                                   where a.Code.Contains(dr["LoanType"].ToString())
                                   select a.Id).Count();
                    if (LoanIDCount < 1)
                    {
                        oApplication.StatusBar.SetText("Provided Loan Type can't be found.Please Provide valid LoanType for Record # " + dr["EmpId"].ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return;
                    }
                    EmpIDCount = (from a in dbHrPayroll.MstEmployee
                                  where a.EmpID == dr["EmpID"].ToString()
                                  select a.ID).Count();
                    if (EmpIDCount < 1)
                    {
                        oApplication.StatusBar.SetText("Provided EmpCode can't be found.Please Provide valid EmpId for Record" + dr["EmpId"].ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return;
                    }
                    if (LoanIDCount != 0 && EmpIDCount != 0)
                    {
                        Int32 LoanID, EmpID;
                        LoanID = (from a in dbHrPayroll.MstLoans
                                  where a.Code.Contains(dr["LoanType"].ToString())
                                  select a.Id).FirstOrDefault();
                        EmpID = (from a in dbHrPayroll.MstEmployee
                                 where a.EmpID == dr["EmpID"].ToString()
                                 select a.ID).FirstOrDefault();
                        TrnsOBLoan oInsert = new TrnsOBLoan();
                        oInsert.EmpID = EmpID;
                        oInsert.LoanID = LoanID;
                        oInsert.LoanAmount = Convert.ToDecimal(dr["LoanAmount"]);
                        oInsert.RecoverdAmount = Convert.ToDecimal(dr["RecoveredAmount"]);
                        oInsert.Installment = Convert.ToDecimal(dr["Installment"]);
                        oInsert.CreateDate = DateTime.Now;
                        //if (!string.IsNullOrEmpty(dr["Month"].ToString()))
                        //{
                        //    DateTime dts = DateTime.ParseExact(dr["Month"].ToString(), "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        //}
                        //oInsert.CreateDate = Convert.ToDateTime(dr["Month"].ToString());
                        oInsert.UpdateDate = DateTime.Now;
                        oInsert.UserID = oCompany.UserName;
                        oInsert.UpdatedBy = oCompany.UserName;

                        var EmpRecord = dbHrPayroll.MstEmployee.Where(e => e.ID == EmpID).FirstOrDefault();
                        if (EmpRecord != null)
                        {
                            TrnsLoan OLoan = new TrnsLoan();
                            OLoan.DocNum = dbHrPayroll.TrnsLoan.Count() + 1;
                            OLoan.Series = -1;
                            OLoan.EmpID = EmpID;
                            OLoan.EmpName = EmpRecord.FirstName + " " + EmpRecord.MiddleName + " " + EmpRecord.LastName;
                            if (EmpRecord.Manager > 0)
                            {
                                OLoan.ManagerID = EmpRecord.Manager;
                                OLoan.ManagerName = (from e in dbHrPayroll.MstEmployee where e.ID == EmpRecord.Manager select (e.FirstName + " " + e.MiddleName + " " + e.LastName)).FirstOrDefault();
                            }
                            OLoan.DateOfJoining = EmpRecord.JoiningDate;
                            OLoan.UserId = oCompany.UserName;
                            OLoan.DesignationID = EmpRecord.DesignationID;
                            OLoan.Designation = EmpRecord.DesignationName;
                            OLoan.Salary = EmpRecord.BasicSalary != 0 ? EmpRecord.BasicSalary : 0;
                            OLoan.OriginatorID = EmpRecord.ID;
                            OLoan.OriginatorName = EmpRecord.FirstName + " " + EmpRecord.MiddleName + " " + EmpRecord.LastName;
                            OLoan.CreateDate = DateTime.Now;
                            OLoan.DocStatus = strDocStatus;
                            OLoan.DocStatusLOV = "DocStatus";
                            OLoan.DocAprStatus = strApprovalStatus;
                            OLoan.DocAprStatusLOV = "ApprovalStatus";
                            dbHrPayroll.TrnsLoan.InsertOnSubmit(OLoan);

                            //Insert LoanDetail Record

                            TrnsLoanDetail oChild = new TrnsLoanDetail();
                            oChild.LoanType = LoanID;
                            oChild.RequestedAmount = Convert.ToDecimal(dr["LoanAmount"]);
                            oChild.Installments = Convert.ToDecimal(dr["Installment"]);
                            //if (!string.IsNullOrEmpty(dr["Active"].ToString()))
                            //{
                            //    string strActive = dr["Active"].ToString();
                            //    oChild.FlgActive = strActive == "Y" ? true : false;
                            //}
                            //else
                            //{
                            //    oChild.FlgActive = true;
                            //}
                            oChild.FlgActive = true;
                            oChild.FlgStopRecovery = false;
                            oChild.ApprovedAmount = oChild.RequestedAmount = Convert.ToDecimal(dr["LoanAmount"]);
                            oChild.ApprovedInstallment = Convert.ToDecimal(dr["Installment"]);
                            oChild.RecoveredAmount = Convert.ToDecimal(dr["RecoveredAmount"]);
                            //oChild.RequiredDate = DateTime.Now;
                            string strTimex = dr["Month"].ToString();
                            oChild.RequiredDate = Convert.ToDateTime(dr["Month"].ToString());
                            oChild.MaturityDate = DateTime.Now;
                            oChild.CreateDate = DateTime.Now;
                            oChild.UserID = oCompany.UserName;
                            OLoan.TrnsLoanDetail.Add(oChild);

                            LoanAmount = 0;
                            RecoveredAmount = 0;
                            DueAmount = 0;
                            if (!string.IsNullOrEmpty(dr["LoanAmount"].ToString()) && !string.IsNullOrEmpty(dr["RecoveredAmount"].ToString()))
                            {
                                LoanAmount = Convert.ToDecimal(dr["LoanAmount"]);
                                RecoveredAmount = Convert.ToDecimal(dr["RecoveredAmount"]);
                                DueAmount = LoanAmount - RecoveredAmount;
                            }
                            TrnsLoanRegister LoanReg = new TrnsLoanRegister();
                            LoanReg.LoanDocNum = dbHrPayroll.TrnsLoanRegister.Count() + 1;
                            LoanReg.LoanAmount = LoanAmount;
                            LoanReg.Series = -1;
                            LoanReg.EmpID = EmpRecord.EmpID;
                            LoanReg.RecoveredAmount = RecoveredAmount;
                            LoanReg.DueAmount = DueAmount;
                            OLoan.TrnsLoanRegister.Add(LoanReg);

                        }
                        else
                        {
                            oApplication.StatusBar.SetText("Provided EmpCode can't be found.Please Provide valid EmpId for Record" + dr["EmpId"].ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            return;
                        }

                        dbHrPayroll.TrnsOBLoan.InsertOnSubmit(oInsert);
                        dbHrPayroll.SubmitChanges();
                    }

                }
                oApplication.StatusBar.SetText("Successfully Added.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("LoanBalanceImport : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void PFBalanceImport()
        {
            try
            {
                foreach (DataRow dr in DtFile.Rows)
                {
                    int chkEmployee = 0;
                    //chkEmployee = (from a in dbHrPayroll.MstEmployee
                    //               where a.EmpID.Contains(dr["EmpID"].ToString())
                    //               select a).Count();
                    chkEmployee = (from a in dbHrPayroll.MstEmployee
                                   where a.EmpID == dr["EmpID"].ToString().Trim()
                                   && a.FlgActive == true
                                   select a).Count();
                    if (chkEmployee != 0)
                    {

                        TrnsOBProvidentFund oInsert = new TrnsOBProvidentFund();
                        //MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee
                        //                    where a.EmpID.Contains(dr["EmpID"].ToString())
                        //                    select a).FirstOrDefault();
                        int empid = (from a in dbHrPayroll.MstEmployee where a.EmpID == (dr["EmpID"].ToString()) select a.ID).FirstOrDefault();

                        var PFOldEmpID = (from a in dbHrPayroll.TrnsOBProvidentFund where a.EmpID == empid select a).FirstOrDefault();
                        if (PFOldEmpID == null)
                        {
                            oInsert.EmpID = empid;
                            oInsert.EmployeeBalance = Convert.ToDecimal(dr["EmployeeBalance"].ToString());
                            oInsert.EmployerBalance = Convert.ToDecimal(dr["EmployerBalance"].ToString());
                            oInsert.CummulativeROI = Convert.ToDecimal(dr["CummulativeROI"].ToString());

                            oInsert.CreateDate = DateTime.Now;
                            oInsert.UpdateDate = DateTime.Now;
                            oInsert.UserID = oCompany.UserName;
                            oInsert.UpdatedBy = oCompany.UserName;

                            dbHrPayroll.TrnsOBProvidentFund.InsertOnSubmit(oInsert);
                            dbHrPayroll.SubmitChanges();
                        }
                        else
                        {
                            PFOldEmpID.EmpID = empid;
                            PFOldEmpID.EmployeeBalance = Convert.ToDecimal(dr["EmployeeBalance"].ToString());
                            PFOldEmpID.EmployerBalance = Convert.ToDecimal(dr["EmployerBalance"].ToString());
                            PFOldEmpID.CummulativeROI = Convert.ToDecimal(dr["CummulativeROI"].ToString());


                            PFOldEmpID.UpdateDate = DateTime.Now;
                            PFOldEmpID.UpdatedBy = oCompany.UserName;


                            dbHrPayroll.SubmitChanges();
                        }
                    }

                }
                oApplication.StatusBar.SetText("Successfully Added.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("PFBalanceImport : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void GratuityImport()
        {
            try
            {
                foreach (DataRow dr in DtFile.Rows)
                {
                    int chkEmployee = 0;
                    chkEmployee = (from a in dbHrPayroll.MstEmployee
                                   where a.EmpID == dr["EmpID"].ToString()
                                   select a).Count();
                    if (chkEmployee != 0)
                    {
                        TrnsOBGratuity oInsert = new TrnsOBGratuity();
                        Int32 empdbid = (from a in dbHrPayroll.MstEmployee
                                         where a.EmpID == dr["EmpID"].ToString()
                                         select a.ID).FirstOrDefault();
                        oInsert.EmpID = empdbid;
                        oInsert.OpeningBalance = Convert.ToDecimal(dr["OpeningBalance"].ToString());
                        oInsert.Month = Convert.ToDateTime(dr["Month"].ToString());
                        dbHrPayroll.TrnsOBGratuity.InsertOnSubmit(oInsert);
                        dbHrPayroll.SubmitChanges();
                    }

                }
                oApplication.StatusBar.SetText("Successfully Added.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("PFBalanceImport : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void LeaveBalanceImport()
        {
            try
            {
                foreach (DataRow dr in DtFile.Rows)
                {
                    int getLeaveID = 0, chkEmployee = 0;
                    getLeaveID = (from a in dbHrPayroll.MstLeaveType
                                  where a.Code == dr["LeaveType"].ToString().Trim()
                                  select a.ID).FirstOrDefault();
                    chkEmployee = (from a in dbHrPayroll.MstEmployee
                                   where a.EmpID == dr["EmpID"].ToString().Trim()
                                   select a.ID).Count();
                    string FiscalYearCode = Convert.ToString(dr["FiscalYear"]);                    
                    var CalenderYear = dbHrPayroll.MstCalendar.Where(c => c.Code == FiscalYearCode.Trim() && c.FlgActive == true).FirstOrDefault();
                    if (CalenderYear == null)
                    {
                        oApplication.StatusBar.SetText("Provided Calender Code can't be found for Record # " + dr["EmpID"].ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        continue;
                    }
                    if (getLeaveID == null || getLeaveID < 1)
                    {
                        oApplication.StatusBar.SetText("Provided Leave Type can't be found.Please Provide valid LeaveType for Record # " + dr["EmpID"].ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        continue;
                    }
                    if (chkEmployee < 1)
                    {
                        oApplication.StatusBar.SetText("Provided EmpCode can't be found.Please Provide valid EmpId for Record # " + dr["EmpID"].ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        continue;
                    }
                    if (getLeaveID != 0 && chkEmployee != 0)
                    {
                        int empid = (from a in dbHrPayroll.MstEmployee where a.EmpID == (dr["EmpID"].ToString()) select a.ID).FirstOrDefault();
                        TrnsOBLeaves oInsert = new TrnsOBLeaves();
                        oInsert.EmpID = empid;
                        oInsert.LeaveID = getLeaveID;
                        oInsert.LeaveBalance = Convert.ToDecimal(dr["LeavesCarryForward"].ToString());
                        oInsert.LeaveAllowance = Convert.ToDecimal(dr["LeavesEntitled"].ToString());
                        oInsert.Month = DateTime.Now; //Convert.ToDateTime(dr["Month"].ToString());
                        oInsert.CreateDate = DateTime.Now;
                        oInsert.UpdateDate = DateTime.Now;
                        oInsert.UserID = oCompany.UserName;
                        oInsert.UpdatedBy = oCompany.UserName;
                        dbHrPayroll.TrnsOBLeaves.InsertOnSubmit(oInsert);
                        dbHrPayroll.SubmitChanges();

                        decimal decLeavesEntitled = Convert.ToDecimal(dr["LeavesEntitled"].ToString());
                        decimal decLeavesCarryForward = Convert.ToDecimal(dr["LeavesCarryForward"].ToString());
                        decimal decLeaveUsed = Convert.ToDecimal(dr["LeavesUsed"].ToString());
                        int cnt = (from a in dbHrPayroll.MstEmployeeLeaves
                                   where a.EmpID == empid
                                   && a.LeaveType == getLeaveID
                                   && a.FromDt == CalenderYear.StartDate
                                   && a.ToDt == CalenderYear.EndDate
                                   select a.ID).Count();
                        if (cnt > 0)
                        {
                            MstEmployeeLeaves oUpd = (from a in dbHrPayroll.MstEmployeeLeaves
                                                      where a.EmpID == empid
                                                      && a.LeaveType == getLeaveID
                                                      && a.FromDt == CalenderYear.StartDate
                                                      && a.ToDt == CalenderYear.EndDate
                                                      select a).FirstOrDefault();
                            oUpd.LeavesEntitled = decLeavesEntitled;
                            oUpd.LeavesUsed = decLeaveUsed;
                            oUpd.LeavesCarryForward = decLeavesCarryForward;
                            oUpd.LeaveCalCode = CalenderYear.Code;
                        }
                        else
                        {
                            MstEmployeeLeaves oNew = new MstEmployeeLeaves();
                            oNew.EmpID = empid;
                            oNew.LeaveType = getLeaveID;
                            oNew.LeavesCarryForward = decLeavesCarryForward;
                            oNew.LeavesEntitled = decLeavesEntitled;
                            oNew.LeavesUsed = decLeaveUsed;
                            oNew.LeavesAllowed = 0;// Convert.ToDecimal(dr["LeaveBalance"].ToString());
                            oNew.FlgActive = true;
                            oNew.FromDt = CalenderYear.StartDate;
                            oNew.ToDt = CalenderYear.EndDate;
                            oNew.LeaveCalCode = CalenderYear.Code;
                            oNew.UserId = oCompany.UserName;
                            oNew.CreateDate = DateTime.Now;
                            oNew.UpdateDate = DateTime.Now;
                            dbHrPayroll.MstEmployeeLeaves.InsertOnSubmit(oNew);

                        }
                        dbHrPayroll.SubmitChanges();
                    }

                }
                oApplication.StatusBar.SetText("Successfully Added.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("LoanBalanceImport : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void LeaveBalanceImportFromLeaveCalendar()
        {
            try
            {
                foreach (DataRow dr in DtFile.Rows)
                {
                    int getLeaveID = 0, chkEmployee = 0;
                    getLeaveID = (from a in dbHrPayroll.MstLeaveType
                                  where a.Code == dr["LeaveType"].ToString().Trim()
                                  select a.ID).FirstOrDefault();
                    chkEmployee = (from a in dbHrPayroll.MstEmployee
                                   where a.EmpID == dr["EmpID"].ToString().Trim()
                                   select a.ID).Count();
                    string FiscalYearCode = Convert.ToString(dr["FiscalYear"]);
                    var CalenderYear = dbHrPayroll.MstLeaveCalendar.Where(c => c.Code == FiscalYearCode.Trim() && c.FlgActive == true).FirstOrDefault();
                    if (CalenderYear == null)
                    {
                        oApplication.StatusBar.SetText("Provided Calender Code can't be found for Record # " + dr["EmpID"].ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        continue;
                    }
                    if (getLeaveID == null || getLeaveID < 1)
                    {
                        oApplication.StatusBar.SetText("Provided Leave Type can't be found.Please Provide valid LeaveType for Record # " + dr["EmpID"].ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        continue;
                    }
                    if (chkEmployee < 1)
                    {
                        oApplication.StatusBar.SetText("Provided EmpCode can't be found.Please Provide valid EmpId for Record # " + dr["EmpID"].ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        continue;
                    }
                    if (getLeaveID != 0 && chkEmployee != 0)
                    {
                        int empid = (from a in dbHrPayroll.MstEmployee where a.EmpID == (dr["EmpID"].ToString()) select a.ID).FirstOrDefault();
                        TrnsOBLeaves oInsert = new TrnsOBLeaves();
                        oInsert.EmpID = empid;
                        oInsert.LeaveID = getLeaveID;
                        oInsert.LeaveBalance = Convert.ToDecimal(dr["LeavesCarryForward"].ToString());
                        oInsert.LeaveAllowance = Convert.ToDecimal(dr["LeavesEntitled"].ToString());
                        oInsert.Month = DateTime.Now; //Convert.ToDateTime(dr["Month"].ToString());
                        oInsert.CreateDate = DateTime.Now;
                        oInsert.UpdateDate = DateTime.Now;
                        oInsert.UserID = oCompany.UserName;
                        oInsert.UpdatedBy = oCompany.UserName;
                        dbHrPayroll.TrnsOBLeaves.InsertOnSubmit(oInsert);
                        dbHrPayroll.SubmitChanges();

                        decimal decLeavesEntitled = Convert.ToDecimal(dr["LeavesEntitled"].ToString());
                        decimal decLeavesCarryForward = Convert.ToDecimal(dr["LeavesCarryForward"].ToString());
                        decimal decLeaveUsed = Convert.ToDecimal(dr["LeavesUsed"].ToString());
                        int cnt = (from a in dbHrPayroll.MstEmployeeLeaves
                                   where a.EmpID == empid
                                   && a.LeaveType == getLeaveID
                                   && a.FromDt == CalenderYear.StartDate
                                   && a.ToDt == CalenderYear.EndDate
                                   select a.ID).Count();
                        if (cnt > 0)
                        {
                            MstEmployeeLeaves oUpd = (from a in dbHrPayroll.MstEmployeeLeaves
                                                      where a.EmpID == empid
                                                      && a.LeaveType == getLeaveID
                                                      && a.FromDt == CalenderYear.StartDate
                                                      && a.ToDt == CalenderYear.EndDate
                                                      select a).FirstOrDefault();
                            oUpd.LeavesEntitled = decLeavesEntitled;
                            oUpd.LeavesUsed = decLeaveUsed;
                            oUpd.LeavesCarryForward = decLeavesCarryForward;
                            oUpd.LeaveCalCode = CalenderYear.Code;
                        }
                        else
                        {
                            MstEmployeeLeaves oNew = new MstEmployeeLeaves();
                            oNew.EmpID = empid;
                            oNew.LeaveType = getLeaveID;
                            oNew.LeavesCarryForward = decLeavesCarryForward;
                            oNew.LeavesEntitled = decLeavesEntitled;
                            oNew.LeavesUsed = decLeaveUsed;
                            oNew.LeavesAllowed = 0;// Convert.ToDecimal(dr["LeaveBalance"].ToString());
                            oNew.FlgActive = true;
                            oNew.FromDt = CalenderYear.StartDate;
                            oNew.ToDt = CalenderYear.EndDate;
                            oNew.LeaveCalCode = CalenderYear.Code;
                            oNew.UserId = oCompany.UserName;
                            oNew.CreateDate = DateTime.Now;
                            oNew.UpdateDate = DateTime.Now;
                            dbHrPayroll.MstEmployeeLeaves.InsertOnSubmit(oNew);

                        }
                        dbHrPayroll.SubmitChanges();
                    }

                }
                oApplication.StatusBar.SetText("Successfully Added.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("LoanBalanceImport : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void ArrearImport()
        {
            try
            {
                foreach (DataRow dr in DtFile.Rows)
                {
                    int getArrearID = 0, chkEmployee = 0;
                    getArrearID = (from a in dbHrPayroll.MstArrears
                                   where a.Code == dr["ArrearType"].ToString()
                                   select a.ID).FirstOrDefault();
                    chkEmployee = (from a in dbHrPayroll.MstEmployee
                                   where a.ID == Convert.ToInt32(dr["EmpID"].ToString())
                                   select a.ID).Count();
                    if (getArrearID != 0 && chkEmployee != 0)
                    {
                        TrnsOBArrears oInsert = new TrnsOBArrears();
                        oInsert.EmpID = Convert.ToInt32(dr["EmpID"]);
                        oInsert.ArrearID = Convert.ToInt32(getArrearID);
                        oInsert.OpeningBalance = Convert.ToDecimal(dr["OpeningBalance"].ToString());
                        oInsert.Month = Convert.ToDateTime(dr["Month"].ToString());
                        dbHrPayroll.TrnsOBArrears.InsertOnSubmit(oInsert);
                        dbHrPayroll.SubmitChanges();
                    }

                }
                oApplication.StatusBar.SetText("Successfully Added.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("ArrearImport : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void ContributionImport()
        {
            try
            {
                foreach (DataRow dr in DtFile.Rows)
                {
                    int getContributionID = 0, chkEmployee = 0;
                    getContributionID = (from a in dbHrPayroll.MstElements
                                         where a.ElementName == dr["ContributionType"].ToString()
                                         select a.Id).FirstOrDefault();

                    //chkEmployee = (from a in dbHrPayroll.MstEmployee
                    //               where a.EmpID == Convert.ToString(dr["EmpID"].ToString())
                    //               select a.ID).Count();
                    chkEmployee = (from a in dbHrPayroll.MstEmployee
                                   where a.EmpID == dr["EmpID"].ToString().Trim()
                                   && a.FlgActive == true
                                   select a).Count();
                    if (getContributionID != 0 && chkEmployee != 0)
                    {
                        int empid = (from a in dbHrPayroll.MstEmployee where a.EmpID == (dr["EmpID"].ToString()) select a.ID).FirstOrDefault();

                        var ContributionOldEmpID = (from a in dbHrPayroll.TrnsOBContribution where a.EmpID == empid select a).FirstOrDefault();
                        if (ContributionOldEmpID == null)
                        {
                            TrnsOBContribution oInsert = new TrnsOBContribution();
                            oInsert.EmpID = empid;
                            oInsert.ContributionID = Convert.ToInt32(getContributionID);
                            oInsert.Balance = Convert.ToDecimal(dr["OpeningBalance"].ToString());
                            if (!string.IsNullOrEmpty(dr["Month"].ToString()))
                            {
                                DateTime dts = DateTime.ParseExact(dr["Month"].ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                oInsert.Month = dts;
                            }
                            else
                            {
                                oInsert.Month = DateTime.Now;
                            }

                            oInsert.UserID = oCompany.UserName;
                            oInsert.CreateDate = DateTime.Now;
                            dbHrPayroll.TrnsOBContribution.InsertOnSubmit(oInsert);
                            dbHrPayroll.SubmitChanges();
                        }
                        else
                        {
                            ContributionOldEmpID.ContributionID = Convert.ToInt32(getContributionID);
                            ContributionOldEmpID.Balance = Convert.ToDecimal(dr["OpeningBalance"].ToString());
                            if (!string.IsNullOrEmpty(dr["Month"].ToString()))
                            {
                                DateTime dts = DateTime.ParseExact(dr["Month"].ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                ContributionOldEmpID.Month = dts;
                            }
                            else
                            {
                                ContributionOldEmpID.Month = DateTime.Now;
                            }

                            ContributionOldEmpID.UpdatedBy = oCompany.UserName;
                            ContributionOldEmpID.UpdateDate = DateTime.Now;

                            dbHrPayroll.SubmitChanges();

                        }
                    }

                }
                oApplication.StatusBar.SetText("Successfully Added.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("LoanBalanceImport : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void SalaryImport()
        {
            try
            {
                foreach (DataRow dr in DtFile.Rows)
                {
                    int chkEmployee = 0;
                    chkEmployee = (from a in dbHrPayroll.MstEmployee
                                   where a.EmpID == dr["EmpID"].ToString()
                                   select a.ID).Count();
                    if (chkEmployee != 0)
                    {
                        TrnsOBSalary oInsert = new TrnsOBSalary();
                        int empid = (from a in dbHrPayroll.MstEmployee where a.EmpID == dr["EmpID"].ToString() select a.ID).FirstOrDefault();
                        oInsert.EmpID = empid;
                        oInsert.SalaryBalance = Convert.ToDecimal(dr["OpeningBalance"].ToString());
                        oInsert.Month = DateTime.Now; //Convert.ToDateTime(dr["Month"].ToString());
                        oInsert.CreateDate = DateTime.Now;
                        oInsert.UpdateDate = DateTime.Now;
                        oInsert.UserId = oCompany.UserName;
                        oInsert.UpdatedBy = oCompany.UserName;
                        dbHrPayroll.TrnsOBSalary.InsertOnSubmit(oInsert);
                        dbHrPayroll.SubmitChanges();
                    }
                    else
                    {
                        oApplication.StatusBar.SetText("Emp Not found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    }

                }
                oApplication.StatusBar.SetText("Successfully Added.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("SalaryImport : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void TaxBalanceImport()
        {
            try
            {
                foreach (DataRow dr in DtFile.Rows)
                {
                    int chkEmployee = 0;
                    chkEmployee = (from a in dbHrPayroll.MstEmployee
                                   where a.EmpID == dr["EmpID"].ToString()
                                    && a.FlgActive == true
                                   select a.ID).Count();
                    if (chkEmployee != 0)
                    {
                        int empid = (from a in dbHrPayroll.MstEmployee where a.EmpID == dr["EmpID"].ToString() select a.ID).FirstOrDefault();


                        var TaxOldEmpID = (from a in dbHrPayroll.TrnsOBTax where a.EmpID == empid select a).FirstOrDefault();
                        if (TaxOldEmpID == null)
                        {
                            TrnsOBTax oInsert = new TrnsOBTax();
                            oInsert.EmpID = empid;
                            oInsert.TaxBalance = Convert.ToDecimal(dr["OpeningBalance"].ToString());
                            //oInsert.Month = DateTime.Now; // Convert.ToDateTime(dr["Month"].ToString());
                            if (!string.IsNullOrEmpty(dr["Month"].ToString()))
                            {
                                DateTime dts = DateTime.ParseExact(dr["Month"].ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                oInsert.Month = dts;
                            }
                            else
                            {
                                oInsert.Month = DateTime.Now;
                            }
                            oInsert.UserID = oCompany.UserName;
                            oInsert.CreateDate = DateTime.Now;
                            dbHrPayroll.TrnsOBTax.InsertOnSubmit(oInsert);
                            dbHrPayroll.SubmitChanges();
                        }
                        else
                        {
                            TaxOldEmpID.EmpID = empid;
                            TaxOldEmpID.TaxBalance = Convert.ToDecimal(dr["OpeningBalance"].ToString());
                            if (!string.IsNullOrEmpty(dr["Month"].ToString()))
                            {
                                DateTime dts = DateTime.ParseExact(dr["Month"].ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                TaxOldEmpID.Month = dts;
                            }
                            else
                            {
                                TaxOldEmpID.Month = DateTime.Now;
                            }
                            TaxOldEmpID.UpdatedBy = oCompany.UserName;
                            TaxOldEmpID.UpdateDate = DateTime.Now;
                            dbHrPayroll.SubmitChanges();
                        }
                    }

                }
                oApplication.StatusBar.SetText("Successfully Added.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("TaxBalanceImport : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        #endregion

    }
}
