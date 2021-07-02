using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using DIHRMS.Custom;
using System.Data;
using System.Collections;
using System.Data.OleDb;
using System.Data.SqlClient;

namespace ACHR.Screen
{
    class frm_DBConn : HRMSBaseForm
    {
        
        #region "Global Variable Area"

        SAPbouiCOM.Button btnAccess, btnTConn, btnSTC, btnSGC, btCancel, btnImport, btnSave, btnrfrsh;
        SAPbouiCOM.EditText txtFileLoc, txtTblName, txtUserName, txtPassword, txtSqlSN, txtSqlUN, txtSqlPas, txtSqlDB, txtsqlTN, txtMachineType, txtFromDate, txtToDate;
        SAPbouiCOM.Item ItxtFileLoc, ItxtTblName, ItxtUserName, ItxtPassword, ItxtSqlSN, ItxtSqlUN, ItxtSqlPas, ItxtSqlDB, ItxtsqlTN, Icbdate; 
        SAPbouiCOM.CheckBox chkAccss, chkSQL;
        SAPbouiCOM.DataTable dtColumnMap;
        SAPbouiCOM.Matrix grdColumnMapp;
        SAPbouiCOM.Columns oColumns;
        private SAPbouiCOM.ComboBox cmbDate, cmbEmpID;
        SAPbouiCOM.Column oColumn, clNo, DestColumn, SourceColumn;      
        DataTable dtTempRecord = new DataTable();
        string EmpCode, InOut, PunchDate, PunchTime;
        string  DateIn, TimeIn, DateOut, TimeOut;
        string PunchDateTime;
        string DateTimeIn, DateTimeOut;
        string CostCenter;
        string ValidEmployeeList;

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
                oApplication.StatusBar.SetText("Form: Frm_DBConn Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            try
            {
                switch (pVal.ItemUID)
                {
                    case "btId":
                        picMachineType();
                        break;
                    case "btnAcc":
                        PopulateSourceColumnComboFROMMSACCESS();
                        break;
                    case "btnSave":
                        SaveRecords();
                        break;
                    case "btPick":
                        getFileName();
                        break;
                    case "btnGC":
                        PopulateSourceColumnComboFROMSQLSERVER();
                        break;
                    case "btrfrsh":
                        DeleteRecordFromTempTable();
                        break;
                    case "btnImprt":
                        if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_OK_MODE)
                            return;
                        else
                            ImportDataFromMachine();
                        break;                   
                    case "btnTConn":
                        TestMSACCESSConnection();
                        break;
                    case "btnSTC":
                        TestSQLConnection();
                        break;
                    case "chkAcc":
                        ActivateAccessFields();
                        break;
                    case "chkSQL":
                        ActivateSQLFields();
                        break;
                    case "2":

                        break;
                    default:
                        break;
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_EmpMst Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        #endregion

        #region "Local Methods"

        public void InitiallizeForm1()
        {
            try
            {
                btnTConn = oForm.Items.Item("btnTConn").Specific;
                btnAccess = oForm.Items.Item("btnAcc").Specific;
                btnSTC = oForm.Items.Item("btnSTC").Specific;
                btnSGC = oForm.Items.Item("btnGC").Specific;
                btnImport = oForm.Items.Item("btnImprt").Specific;     
                btCancel = oForm.Items.Item("2").Specific;
                btnSave = oForm.Items.Item("btnSave").Specific;
                btnrfrsh = oForm.Items.Item("btrfrsh").Specific;

                cmbDate = oForm.Items.Item("cb_date").Specific;
                oForm.DataSources.UserDataSources.Add("cb_date", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cmbDate.DataBind.SetBound(true, "", "cb_date");
                Icbdate = oForm.Items.Item("cb_date");

                oForm.DataSources.UserDataSources.Add("txtMType", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 100);
                txtMachineType = oForm.Items.Item("txtMType").Specific;
                txtMachineType.DataBind.SetBound(true, "", "txtMType");

                //Initializing ChechkBoxes

                oForm.DataSources.UserDataSources.Add("txtFdt", SAPbouiCOM.BoDataType.dt_DATE);
                txtFromDate = oForm.Items.Item("txtFdt").Specific;
                txtFromDate.DataBind.SetBound(true, "", "txtFdt");

                oForm.DataSources.UserDataSources.Add("txtTDt", SAPbouiCOM.BoDataType.dt_DATE);
                txtToDate = oForm.Items.Item("txtTDt").Specific;
                txtToDate.DataBind.SetBound(true, "", "txtTDt");

                oForm.DataSources.UserDataSources.Add("chkAcc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); 
                chkAccss = oForm.Items.Item("chkAcc").Specific;                
                chkAccss.DataBind.SetBound(true, "", "chkAcc");

                oForm.DataSources.UserDataSources.Add("chkSQL", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); 
                chkSQL = oForm.Items.Item("chkSQL").Specific;               
                chkSQL.DataBind.SetBound(true, "", "chkSQL");

                //Initializing TextBoxes
                txtFileLoc = oForm.Items.Item("txtFLoc").Specific;
                oForm.DataSources.UserDataSources.Add("txtFLoc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                ItxtFileLoc = oForm.Items.Item("txtFLoc");
                txtFileLoc.DataBind.SetBound(true, "", "txtFLoc");
                             
                txtTblName = oForm.Items.Item("txtTabN").Specific;
                oForm.DataSources.UserDataSources.Add("txtTabN", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                ItxtTblName = oForm.Items.Item("txtTabN");
                txtTblName.DataBind.SetBound(true, "", "txtTabN");
               
                txtUserName = oForm.Items.Item("txtUsrN").Specific;
                oForm.DataSources.UserDataSources.Add("txtUsrN", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                ItxtUserName = oForm.Items.Item("txtUsrN");
                txtUserName.DataBind.SetBound(true, "", "txtUsrN");
               
                txtPassword = oForm.Items.Item("txtPass").Specific;
                oForm.DataSources.UserDataSources.Add("txtPass", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                ItxtPassword = oForm.Items.Item("txtPass");
                txtPassword.DataBind.SetBound(true, "", "txtPass");

                txtSqlSN = oForm.Items.Item("txtSSN").Specific;
                oForm.DataSources.UserDataSources.Add("txtSSN", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                ItxtSqlSN = oForm.Items.Item("txtSSN");
                txtSqlSN.DataBind.SetBound(true, "", "txtSSN");

                txtSqlUN = oForm.Items.Item("txtSUN").Specific;
                oForm.DataSources.UserDataSources.Add("txtSUN", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                ItxtSqlUN = oForm.Items.Item("txtSUN");
                txtSqlUN.DataBind.SetBound(true, "", "txtSUN");

                txtSqlPas = oForm.Items.Item("txtSPas").Specific;
                oForm.DataSources.UserDataSources.Add("txtSPas", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                ItxtSqlPas = oForm.Items.Item("txtSPas");
                txtSqlPas.DataBind.SetBound(true, "", "txtSPas");

                txtSqlDB = oForm.Items.Item("txtSDB").Specific;
                oForm.DataSources.UserDataSources.Add("txtSDB", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                ItxtSqlDB = oForm.Items.Item("txtSDB");
                txtSqlDB.DataBind.SetBound(true, "", "txtSDB");

                txtsqlTN = oForm.Items.Item("txtTN").Specific;
                oForm.DataSources.UserDataSources.Add("txtTN", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                ItxtsqlTN = oForm.Items.Item("txtTN");
                txtFileLoc.DataBind.SetBound(true, "", "txtTN");
                 
 
                InitiallizegridMatrix();
                GetSaveRecords();
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("InitializeFrom Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public void InitiallizeForm()
        {
            try
            {
                btnTConn = oForm.Items.Item("btnTConn").Specific;
                btnAccess = oForm.Items.Item("btnAcc").Specific;
                btnSTC = oForm.Items.Item("btnSTC").Specific;
                btnSGC = oForm.Items.Item("btnGC").Specific;
                btnImport = oForm.Items.Item("btnImprt").Specific;
                btCancel = oForm.Items.Item("2").Specific;
                btnSave = oForm.Items.Item("btnSave").Specific;
                btnrfrsh = oForm.Items.Item("btrfrsh").Specific;
                cmbDate = oForm.Items.Item("cb_date").Specific;
                cmbEmpID = oForm.Items.Item("cbempid").Specific;
                oForm.DataSources.UserDataSources.Add("txtMType", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 100);
                txtMachineType = oForm.Items.Item("txtMType").Specific;
                txtMachineType.DataBind.SetBound(true, "", "txtMType");

                //Initializing ChechkBoxes

                oForm.DataSources.UserDataSources.Add("txtFdt", SAPbouiCOM.BoDataType.dt_DATE);
                txtFromDate = oForm.Items.Item("txtFdt").Specific;
                txtFromDate.DataBind.SetBound(true, "", "txtFdt");

                oForm.DataSources.UserDataSources.Add("txtTDt", SAPbouiCOM.BoDataType.dt_DATE);
                txtToDate = oForm.Items.Item("txtTDt").Specific;
                txtToDate.DataBind.SetBound(true, "", "txtTDt");

                oForm.DataSources.UserDataSources.Add("chkAcc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                chkAccss = oForm.Items.Item("chkAcc").Specific;
                chkAccss.DataBind.SetBound(true, "", "chkAcc");

                oForm.DataSources.UserDataSources.Add("chkSQL", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                chkSQL = oForm.Items.Item("chkSQL").Specific;
                chkSQL.DataBind.SetBound(true, "", "chkSQL");

                //Initializing TextBoxes
                txtFileLoc = oForm.Items.Item("txtFLoc").Specific;
                ItxtFileLoc = oForm.Items.Item("txtFLoc");

                txtTblName = oForm.Items.Item("txtTabN").Specific;
                ItxtTblName = oForm.Items.Item("txtTabN");

                txtUserName = oForm.Items.Item("txtUsrN").Specific;
                ItxtUserName = oForm.Items.Item("txtUsrN");

                txtPassword = oForm.Items.Item("txtPass").Specific;
                ItxtPassword = oForm.Items.Item("txtPass");

                txtSqlSN = oForm.Items.Item("txtSSN").Specific;
                ItxtSqlSN = oForm.Items.Item("txtSSN");

                txtSqlUN = oForm.Items.Item("txtSUN").Specific;
                ItxtSqlUN = oForm.Items.Item("txtSUN");

                txtSqlPas = oForm.Items.Item("txtSPas").Specific;
                ItxtSqlPas = oForm.Items.Item("txtSPas");

                txtSqlDB = oForm.Items.Item("txtSDB").Specific;
                ItxtSqlDB = oForm.Items.Item("txtSDB");

                txtsqlTN = oForm.Items.Item("txtTN").Specific;
                ItxtsqlTN = oForm.Items.Item("txtTN");


                InitiallizegridMatrix();
                GetSaveRecords();
                FillValidEmployeeList();
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("InitializeFrom Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void InitiallizegridMatrix()
        {
            try
            {
                dtColumnMap = oForm.DataSources.DataTables.Add("ColumnMapping");
                dtColumnMap.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtColumnMap.Columns.Add("DestColumn", SAPbouiCOM.BoFieldsType.ft_Text);
                dtColumnMap.Columns.Add("SourceColumn", SAPbouiCOM.BoFieldsType.ft_Text);

                grdColumnMapp = (SAPbouiCOM.Matrix)oForm.Items.Item("grdMapp").Specific;
                oColumns = (SAPbouiCOM.Columns)grdColumnMapp.Columns;

                oColumn = oColumns.Item("clNo");
                clNo = oColumn;
                oColumn.DataBind.Bind("ColumnMapping", "No");

                oColumn = oColumns.Item("cl_dest");
                DestColumn = oColumn;
                oColumn.DataBind.Bind("ColumnMapping", "DestColumn");

                oColumn = oColumns.Item("cl_Sorc");
                SourceColumn = oColumn;
                oColumn.DataBind.Bind("ColumnMapping", "SourceColumn");
               // Initializing Second Grid

                //dtcolumnMap2 = oForm.DataSources.DataTables.Add("ColumnMapping2");
                //dtcolumnMap2.Columns.Add("No2", SAPbouiCOM.BoFieldsType.ft_Integer);
                //dtcolumnMap2.Columns.Add("DestColumn2", SAPbouiCOM.BoFieldsType.ft_Text);
                //dtcolumnMap2.Columns.Add("SourceColumn2", SAPbouiCOM.BoFieldsType.ft_Text);

                //grdColumnMapp2 = (SAPbouiCOM.Matrix)oForm.Items.Item("grdMapp2").Specific;
                //oColumns = (SAPbouiCOM.Columns)grdColumnMapp2.Columns;

                //oColumn = oColumns.Item("clNo2");
                //clNo2 = oColumn;
                //oColumn.DataBind.Bind("ColumnMapping2", "No2");

                //oColumn = oColumns.Item("cl_dest2");
                //DestColumn2 = oColumn;
                //oColumn.DataBind.Bind("ColumnMapping2", "DestColumn2");

                //oColumn = oColumns.Item("cl_Sorc2");
                //SourceColumn2 = oColumn;
                //oColumn.DataBind.Bind("ColumnMapping2", "SourceColumn2");

                FillColumnMappingGridM1();           

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void FillColumnMappingGridM1()
        {
            dtColumnMap.Rows.Clear();
            try
            {
                int i = 0;
                ArrayList list = new ArrayList();
                list.Add("EmpID");
                //list.Add("PolledDate");
                list.Add("In_Out");
                list.Add("PunchedDate");
                list.Add("PunchedTime");

                if (list.Count > 0)
                {
                    foreach (var v in list)
                    {
                        dtColumnMap.Rows.Add(1);
                        dtColumnMap.SetValue("No", i, i + 1);                        
                        dtColumnMap.SetValue("DestColumn", i, v.ToString());                        
                        i += 1;
                    }
                    grdColumnMapp.LoadFromDataSource();
                }                                         
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("FillColumnMappingGrid Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void FillColumnMappingGridM2()
        {
            dtColumnMap.Rows.Clear();
            try
            {
                int i = 0;
                ArrayList list = new ArrayList();
                list.Add("EmpID");               
                list.Add("In_Out");
                list.Add("PunchedDateTime");           
                if (list.Count > 0)
                {
                    foreach (var v in list)
                    {
                        dtColumnMap.Rows.Add(1);
                        dtColumnMap.SetValue("No", i, i + 1);
                        dtColumnMap.SetValue("DestColumn", i, v.ToString());
                        i += 1;
                    }
                    grdColumnMapp.LoadFromDataSource();
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("FillColumnMappingGridM2 Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void FillColumnMappingGridM3()
        {
            dtColumnMap.Rows.Clear();
            try
            {
                int i = 0;
                ArrayList list = new ArrayList();
                list.Add("EmpID");              
                list.Add("DateIn");
                list.Add("TimeIn");
                list.Add("DateOut");
                list.Add("TimeOut");

                if (list.Count > 0)
                {
                    foreach (var v in list)
                    {
                        dtColumnMap.Rows.Add(1);
                        dtColumnMap.SetValue("No", i, i + 1);
                        dtColumnMap.SetValue("DestColumn", i, v.ToString());
                        i += 1;
                    }
                    grdColumnMapp.LoadFromDataSource();
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("FillColumnMappingGridM3 Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void FillColumnMappingGridM4()
        {
            dtColumnMap.Rows.Clear();
            try
            {
                int i = 0;
                ArrayList list = new ArrayList();
                list.Add("EmpID");
                list.Add("FirstName");
                list.Add("MiddleName");
                list.Add("LastName");
                if (list.Count > 0)
                {
                    foreach (var v in list)
                    {
                        dtColumnMap.Rows.Add(1);
                        dtColumnMap.SetValue("No", i, i + 1);
                        dtColumnMap.SetValue("DestColumn", i, v.ToString());
                        i += 1;
                    }
                    grdColumnMapp.LoadFromDataSource();
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("FillColumnMappingGridM3 Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillColumnMappingGridM5()
        {
            dtColumnMap.Rows.Clear();
            try
            {
                int i = 0;
                ArrayList list = new ArrayList();
                list.Add("EmpID");
                list.Add("InOut");
                list.Add("AttandanceDate");
                list.Add("AttandanceTime");
                list.Add("CostCenter");
                if (list.Count > 0)
                {
                    foreach (var v in list)
                    {
                        dtColumnMap.Rows.Add(1);
                        dtColumnMap.SetValue("No", i, i + 1);
                        dtColumnMap.SetValue("DestColumn", i, v.ToString());
                        i += 1;
                    }
                    grdColumnMapp.LoadFromDataSource();
                }  
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("FillColumnMappingGridM4 Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void getFileName()
        {
            string fileName = Program.objHrmsUI.FindFile();
            txtFileLoc.Value = fileName;

        }
        
        private void PopulateSourceColumnComboFROMMSACCESS()
        {
            try
            {
                string strFileLoc = txtFileLoc.Value;
                string strTableName = txtTblName.Value;
                if (!string.IsNullOrEmpty(strFileLoc) && !string.IsNullOrEmpty(strTableName))
                {

                    using (OleDbConnection conn = new OleDbConnection(String.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + strFileLoc + ";Persist Security Info=False")))
                    {
                        conn.Open();
                        DataTable dt = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new object[] {
				                    null,
				                    null,
				                    strTableName,
				                    null
			                    });
                        conn.Close();
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            int count = SourceColumn.ValidValues.Count;
                            for (int i = 1; i <= count; i++)
                            {
                                SourceColumn.ValidValues.Remove(count - i, SAPbouiCOM.BoSearchKey.psk_Index);
                            }
                            int Count2 = cmbDate.ValidValues.Count;
                            for (int i = 1; i <= Count2; i++)
                            {
                                cmbDate.ValidValues.Remove(Count2 - i, SAPbouiCOM.BoSearchKey.psk_Index);
                            }
                            int Count3 = cmbEmpID.ValidValues.Count;
                            for (int i = 1; i <= Count3; i++)
                            {
                                cmbEmpID.ValidValues.Remove(Count3 - i, SAPbouiCOM.BoSearchKey.psk_Index);
                            }
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                string strCollumnName = dt.Rows[i]["column_name"].ToString();
                                if (!string.IsNullOrEmpty(strCollumnName))
                                {
                                    SourceColumn.ValidValues.Add(strCollumnName, strCollumnName);
                                    cmbDate.ValidValues.Add(strCollumnName, strCollumnName);
                                    cmbEmpID.ValidValues.Add(strCollumnName, strCollumnName);
                                }
                            }
                            //oApplication.SetStatusBarMessage(Program.objHrmsUI.getStrMsg("ColumnPopulatedSuccessfully"), SAPbouiCOM.BoMessageTime.bmt_Short, false);
                            oApplication.StatusBar.SetText("Column(s) Populated Successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                        }
                    }
                }
                else
                {
                    oApplication.StatusBar.SetText("Please Provide valid Information to Fetch Column(s) Record.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("PopulateSourceColumnComboFROMMSACCESS Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void PopulateSourceColumnComboFROMSQLSERVER()
        {
            try
            {
                string strServerName = txtSqlSN.Value;
                string strUserName = txtSqlUN.Value;
                string strPassword = txtSqlPas.Value;
                string strDBName = txtSqlDB.Value;
                string strTableName=txtsqlTN.Value;
                 DataTable dt =new DataTable();
                 if (!string.IsNullOrEmpty(strServerName) && !string.IsNullOrEmpty(strUserName) && !string.IsNullOrEmpty(strPassword) && !string.IsNullOrEmpty(strDBName))
                 {
                     using (SqlConnection conn = new SqlConnection(String.Format("Server=" + strServerName + ";Database=" + strDBName + ";User Id=" + strUserName + ";Password=" + strPassword + ";")))
                     {
                         try
                         {
                             conn.Open();
                             string sqlQuery = "SELECT column_name FROM information_schema.columns WHERE table_name = '" + strTableName + "'";
                             SqlDataAdapter da = new SqlDataAdapter(sqlQuery, conn);
                             da.Fill(dt);
                             if (dt != null && dt.Rows.Count > 0)
                             {
                                 
                                 int count = SourceColumn.ValidValues.Count;
                                 for (int i = 1; i <= count; i++)
                                 {
                                     SourceColumn.ValidValues.Remove(count - i, SAPbouiCOM.BoSearchKey.psk_Index);
                                 }
                                 int Count2 = cmbDate.ValidValues.Count;
                                 for (int i = 1; i <= Count2; i++)
                                 {
                                     cmbDate.ValidValues.Remove(Count2 - i, SAPbouiCOM.BoSearchKey.psk_Index);
                                 }
                                 int Count3 = cmbEmpID.ValidValues.Count;
                                 for (int i = 1; i <= Count3; i++)
                                 {
                                     cmbEmpID.ValidValues.Remove(Count3 - i, SAPbouiCOM.BoSearchKey.psk_Index);
                                 }
                                 for (int i = 0; i < dt.Rows.Count; i++)
                                 {
                                     string strCollumnName = dt.Rows[i]["column_name"].ToString();
                                     if (!string.IsNullOrEmpty(strCollumnName))
                                     {
                                         SourceColumn.ValidValues.Add(strCollumnName, strCollumnName);
                                         cmbDate.ValidValues.Add(strCollumnName, strCollumnName);
                                         cmbEmpID.ValidValues.Add(strCollumnName, strCollumnName);
                                     }
                                 }
                                 oApplication.StatusBar.SetText("Column(s) Populated Successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                                // oApplication.SetStatusBarMessage(Program.objHrmsUI.getStrMsg("ColumnPopulatedSuccessfully"), SAPbouiCOM.BoMessageTime.bmt_Short, false);
                             }
                         }
                         catch (Exception Ex)
                         {                             
                             oApplication.StatusBar.SetText("Database sever not found please verify connection.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                         }
                         finally
                         {
                             conn.Close();
                         }
                     }

                 }
                 else
                 {
                     oApplication.StatusBar.SetText("Please Provide valid Information to Fetch Column(s) Record.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                 }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("PopulateSourceColumnCombo Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void ImportDataFromDBM1()
        {
            string LineNumber = "";
            try
            {
                string strFileLoc = txtFileLoc.Value;
                string strTableName = txtTblName.Value;
                string strServerName = txtSqlSN.Value;
                string strUserName = txtSqlUN.Value;
                string strPassword = txtSqlPas.Value;
                string strDBName = txtSqlDB.Value;
                string strSQLTableName = txtsqlTN.Value;
                string dateColumnName = "";
                string empidColumnName = "";
                string WhereClause = "";

                SAPbouiCOM.ProgressBar prog = null;

                string strQuery = "Select ";
                string strSourceCollumnValue = "";
                for (int i = 1; i <= grdColumnMapp.RowCount; i++)
                {
                    strSourceCollumnValue = (grdColumnMapp.Columns.Item("cl_Sorc").Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                    if (!string.IsNullOrEmpty(strSourceCollumnValue))
                    {
                        dtColumnMap.SetValue("SourceColumn", i - 1, strSourceCollumnValue);
                    }
                    else
                    {
                        oApplication.StatusBar.SetText("Please Provide Valid Column(s) Mapping", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return;
                    }
                }
                for (int k = 0; k < dtColumnMap.Rows.Count; k++)
                {
                    strQuery = strQuery + Convert.ToString(dtColumnMap.GetValue("SourceColumn", k)) + ", ";
                }
                if (!string.IsNullOrEmpty(strQuery))
                {
                    try
                    {
                        strQuery = strQuery.Remove(strQuery.Length - 2, 2);
                        if (chkAccss.Checked && !string.IsNullOrEmpty(strFileLoc) && !string.IsNullOrEmpty(strTableName))
                        {
                            using (OleDbConnection Oledbconn = new OleDbConnection(String.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + strFileLoc + ";Persist Security Info=False")))
                            {
                                string SQL = strQuery + " From " + strTableName;
                                if (!string.IsNullOrEmpty(cmbDate.Value.Trim()) && !string.IsNullOrEmpty(txtFromDate.Value.Trim()) && !string.IsNullOrEmpty(txtToDate.Value.Trim()) && !string.IsNullOrEmpty(cmbEmpID.Value))
                                {
                                    dateColumnName = cmbDate.Value.Trim();
                                    empidColumnName = cmbEmpID.Value.Trim();
                                    DateTime startDate = DateTime.ParseExact(txtFromDate.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                    DateTime EndDate = DateTime.ParseExact(txtToDate.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                    WhereClause = " WHERE " + dateColumnName + " >= '" + startDate.ToString("MM/dd/yyyy") + "' AND " + dateColumnName + " <= '" + EndDate.ToString("MM/dd/yyyy") + "'";
                                }
                                if (!string.IsNullOrEmpty(WhereClause))
                                {
                                    SQL = SQL + WhereClause;
                                }
                                OleDbCommand oledbcmdQuery = new OleDbCommand();
                                oledbcmdQuery.CommandType = CommandType.Text;
                                oledbcmdQuery.CommandText = SQL;
                                oledbcmdQuery.Connection = Oledbconn;
                                OleDbDataAdapter da = new OleDbDataAdapter(oledbcmdQuery);
                                da.Fill(dtTempRecord);
                            }
                        }
                        else if (chkSQL.Checked && !string.IsNullOrEmpty(strServerName) && !string.IsNullOrEmpty(strUserName) && !string.IsNullOrEmpty(strDBName) && !string.IsNullOrEmpty(strPassword))
                        {
                            using (SqlConnection sqlconn = new SqlConnection(String.Format("Server=" + strServerName + ";Database=" + strDBName + ";User Id=" + strUserName + ";Password=" + strPassword + ";")))
                            {
                                string SQL = strQuery + " From " + strSQLTableName;
                                if (!string.IsNullOrEmpty(cmbDate.Value) && !string.IsNullOrEmpty(txtFromDate.Value) && !string.IsNullOrEmpty(txtToDate.Value) && !string.IsNullOrEmpty(cmbEmpID.Value))
                                {
                                    dateColumnName = cmbDate.Value.Trim();
                                    empidColumnName = cmbEmpID.Value.Trim();
                                    DateTime startDate = DateTime.ParseExact(txtFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                    DateTime EndDate = DateTime.ParseExact(txtToDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                    //WhereClause = " WHERE " + dateColumnName + " >= '" + startDate.ToString("MM/dd/yyyy") + "' AND " + dateColumnName + " <= '" + EndDate.ToString("MM/dd/yyyy") + "' AND " + empidColumnName + " IN (" + ValidEmployeeList + ")";
                                    WhereClause = " WHERE " + dateColumnName + " >= '" + startDate.ToString("yyyy-MM-dd") + "' AND " + dateColumnName + " <= '" + EndDate.ToString("yyyy-MM-dd") + "' AND " + empidColumnName + " IN (" + ValidEmployeeList + ")";
                                }
                                if (!string.IsNullOrEmpty(WhereClause))
                                {
                                    SQL = SQL + WhereClause;
                                }
                                SqlCommand sqlCmdQuery = new SqlCommand();
                                sqlCmdQuery.CommandType = CommandType.Text;
                                sqlCmdQuery.CommandText = SQL;
                                sqlCmdQuery.Connection = sqlconn;
                                SqlDataAdapter sqlda = new SqlDataAdapter(sqlCmdQuery);
                                sqlda.Fill(dtTempRecord);
                            }
                        }
                        if (dtTempRecord != null && dtTempRecord.Rows.Count > 0)
                        {
                            int totalEmps = dtTempRecord.Rows.Count;
                            prog = oApplication.StatusBar.CreateProgressBar("Importing Employee(s) Attendance Record(s)", totalEmps, false);
                            prog.Value = 0;
                            DateTime startDate = DateTime.ParseExact(txtFromDate.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                            DateTime EndDate = DateTime.ParseExact(txtToDate.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                            var otempData = (from a in dbHrPayroll.TrnsTempAttendance
                                             where a.PunchedDate >= startDate
                                             && a.PunchedDate <= EndDate
                                             select a).ToList();
                            if (otempData.Count > 0)
                            {
                                dbHrPayroll.TrnsTempAttendance.DeleteAllOnSubmit(otempData);
                                dbHrPayroll.SubmitChanges();
                            }

                            for (int i = 0; i < dtTempRecord.Rows.Count; i++)
                            {
                                System.Windows.Forms.Application.DoEvents();
                                prog.Value += 1;

                                EmpCode = dtTempRecord.Rows[i][0].ToString();
                                InOut = dtTempRecord.Rows[i][1].ToString();
                                PunchDate = dtTempRecord.Rows[i][2].ToString();
                                PunchTime = dtTempRecord.Rows[i][3].ToString();

                                LineNumber = i.ToString() + " : " + EmpCode + " " + InOut + " " + PunchDate + " " + PunchTime;
                                
                                if (string.IsNullOrEmpty(PunchTime))
                                {
                                    oApplication.StatusBar.SetText(EmpCode + " PunchTime can't be found. Please Provide valid PunchTime", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    continue;
                                }
                                if (string.IsNullOrEmpty(PunchDate))
                                {
                                    oApplication.StatusBar.SetText(EmpCode + " Date can't be found. Please Provide valid Date", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    continue;
                                }
                                
                                //TimeIn = PunchTime;
                                if (!string.IsNullOrEmpty(PunchTime) && PunchTime.Length > 12)
                                {
                                    DateTime dt = DateTime.Parse(PunchTime);
                                    TimeIn = dt.ToString("HH:mm");
                                }
                                else
                                {
                                    TimeSpan ts = TimeSpan.Parse(PunchTime);
                                    TimeIn= ts.ToString(@"hh\:mm");
                                }

                                var EmpRecord = dbHrPayroll.MstEmployee.Where(e => e.EmpID == EmpCode.Trim()).FirstOrDefault();
                                if (EmpRecord == null)
                                {
                                    oApplication.StatusBar.SetText(EmpCode + " EmpId can't be found. Please Provide valid Employee Code", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    continue;
                                }
                                else
                                {
                                    //if (!EmpRecord.FlgActive.Value)
                                    //{
                                    //    oApplication.StatusBar.SetText("Employee is deactive with EmpCode " + EmpCode, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                    //    return;
                                    //}
                                    if (string.IsNullOrEmpty(InOut))
                                    {
                                        oApplication.StatusBar.SetText("Please provide valid In Out code for Employee " + EmpCode, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                        continue;
                                    }
                                    if (!string.IsNullOrEmpty(InOut))
                                    {
                                        InOut = InOut.Trim().ToUpper();
                                        switch (InOut)
                                        {
                                            case "01":
                                            case "1":
                                            case "02":
                                            case "2":
                                            case "I":
                                            case "U":
                                            case "O":
                                            case "i":
                                            case "u":
                                            case "o":
                                                break;
                                            default:
                                                oApplication.StatusBar.SetText("Please provide valid In Out code for Employee " + EmpCode + " for Date " + PunchDate, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                                continue;
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(InOut) && InOut.Trim() == "01")
                                    {
                                        InOut = "1";
                                    }
                                    else if (!string.IsNullOrEmpty(InOut) && InOut.Trim() == "02")
                                    {
                                        InOut = "2";
                                    }
                                    else if (!string.IsNullOrEmpty(InOut) && (InOut.Trim() == "I" || InOut.Trim() == "i"))
                                    {
                                        InOut = "1";
                                    }
                                    else if (!string.IsNullOrEmpty(InOut) && (InOut.Trim() == "U" || InOut.Trim() == "O"|| InOut.Trim() == "u" || InOut.Trim() == "o"))
                                    {
                                        InOut = "2";
                                    }
                                    //Duplicate Attendance Check
                                    //int tempcheck = 0;
                                    //tempcheck = (from a in dbHrPayroll.TrnsTempAttendance where a.EmpID == EmpCode & a.In_Out == InOut & a.PunchedDate == Convert.ToDateTime(PunchDate) select a).Count();
                                    //if (tempcheck > 0)
                                    //{
                                    //    var OldTempRecords = (from a in dbHrPayroll.TrnsTempAttendance where a.EmpID == EmpCode & a.In_Out == InOut & a.PunchedDate == Convert.ToDateTime(PunchDate) select a).ToList();
                                    //    if (OldTempRecords.Count > 0)
                                    //    {
                                    //        foreach (var oneline in OldTempRecords)
                                    //        {
                                    //            dbHrPayroll.TrnsTempAttendance.DeleteOnSubmit(oneline);
                                    //        }
                                    //        dbHrPayroll.SubmitChanges();
                                    //    }
                                    //}
                                    //
                                    //TimeSpan ts = TimeSpan.Parse(PunchTime);
                                    //PunchTime = ts.ToString(@"hh\:mm");
                                    PunchTime = TimeIn;
                                    TrnsTempAttendance TempAttdance = new TrnsTempAttendance();
                                    //TempAttdance.EmpID = EmpRecord.ID;
                                    TempAttdance.EmpID = EmpRecord.EmpID;
                                    //TempAttdance.PolledDate = Convert.ToDateTime(PolledDate);
                                    TempAttdance.In_Out = InOut.Trim();
                                    TempAttdance.PunchedDate = Convert.ToDateTime(PunchDate).Date;
                                    TempAttdance.PunchedTime = PunchTime.Trim();
                                    TempAttdance.FlgProcessed = false;
                                    TempAttdance.CreatedDate = DateTime.Now;
                                    TempAttdance.UserID = oCompany.UserName;
                                    dbHrPayroll.TrnsTempAttendance.InsertOnSubmit(TempAttdance);
                                    dbHrPayroll.SubmitChanges();
                                }
                            }

                            oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                            oApplication.StatusBar.SetText("Attendacne Imported Successfully", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                            //oApplication.SetStatusBarMessage("Attendacne Imported Successfully", SAPbouiCOM.BoMessageTime.bmt_Short, false);                         
                        }
                        else
                        {
                            oApplication.StatusBar.SetText("No Record Found", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_None);
                        }
                    }
                    catch (Exception Ex)
                    {
                        oApplication.StatusBar.SetText("ImportData M1 Error @ Line : " + LineNumber + " : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    }
                    finally
                    {
                        if (prog != null)
                        {
                            prog.Stop();
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(prog);
                        }
                        prog = null;
                    }
                }
            }

            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("ImportData M1 Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void ImportDataFromDBM2()
        {
            try
            {
                string dateColumnName = "";
                string WhereClause = "";
                string strFileLoc = txtFileLoc.Value;
                string strTableName = txtTblName.Value;
                string strServerName = txtSqlSN.Value;
                string strUserName = txtSqlUN.Value;
                string strPassword = txtSqlPas.Value;
                string strDBName = txtSqlDB.Value;
                string strSQLTableName = txtsqlTN.Value;
                SAPbouiCOM.ProgressBar prog = null;
                string empidColumnName = "";
                string strQuery = "Select ";
                string strSourceCollumnValue = "";
                for (int i = 1; i <= grdColumnMapp.RowCount; i++)
                {
                    strSourceCollumnValue = (grdColumnMapp.Columns.Item("cl_Sorc").Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                    if (!string.IsNullOrEmpty(strSourceCollumnValue))
                    {
                        dtColumnMap.SetValue("SourceColumn", i - 1, strSourceCollumnValue);
                    }
                    else
                    {
                        oApplication.StatusBar.SetText("Please Provide Valid Column(s) Mapping", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return;
                    }
                }
                for (int k = 0; k < dtColumnMap.Rows.Count; k++)
                {
                    strQuery = strQuery + Convert.ToString(dtColumnMap.GetValue("SourceColumn", k)) + ", ";
                }
                if (!string.IsNullOrEmpty(strQuery))
                {
                    try
                    {
                        strQuery = strQuery.Remove(strQuery.Length - 2, 2);
                        if (chkAccss.Checked && !string.IsNullOrEmpty(strFileLoc) && !string.IsNullOrEmpty(strTableName))
                        {
                            using (OleDbConnection Oledbconn = new OleDbConnection(String.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + strFileLoc + ";Persist Security Info=False")))
                            {
                                string SQL = strQuery + " From " + strTableName;
                                if (!string.IsNullOrEmpty(cmbDate.Value.Trim()) && !string.IsNullOrEmpty(txtFromDate.Value.Trim()) && !string.IsNullOrEmpty(txtToDate.Value.Trim()) && !string.IsNullOrEmpty(cmbEmpID.Value))
                                {
                                    dateColumnName = cmbDate.Value.Trim();
                                    DateTime startDate = DateTime.ParseExact(txtFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                    DateTime EndDate = DateTime.ParseExact(txtToDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                    WhereClause = " WHERE " + dateColumnName + " >= #" + startDate.Date.ToString("yyyy-MM-dd hh:mm:ss") + "#  AND  " + dateColumnName + " <= #" + EndDate.Date.ToString("yyyy-MM-dd hh:mm:ss") + "#";
                                    //WhereClause = " WHERE " + dateColumnName + " >= #" + startDate.Date.ToString("yyyy-MM-dd") + "#  AND  " + dateColumnName + " <= #" + EndDate.ToString("yyyy-MM-dd") + "#";
                                }
                                if (!string.IsNullOrEmpty(WhereClause))
                                {
                                    SQL = SQL + WhereClause;
                                }
                                OleDbCommand oledbcmdQuery = new OleDbCommand();
                                oledbcmdQuery.CommandType = CommandType.Text;
                                oledbcmdQuery.CommandText = SQL;
                                oledbcmdQuery.Connection = Oledbconn;
                                OleDbDataAdapter da = new OleDbDataAdapter(oledbcmdQuery);
                                da.Fill(dtTempRecord);
                            }
                        }
                        else if (chkSQL.Checked && !string.IsNullOrEmpty(strServerName) && !string.IsNullOrEmpty(strUserName) && !string.IsNullOrEmpty(strDBName) && !string.IsNullOrEmpty(strPassword))
                        {
                            using (SqlConnection sqlconn = new SqlConnection(String.Format("Server=" + strServerName + ";Database=" + strDBName + ";User Id=" + strUserName + ";Password=" + strPassword + ";")))
                            {
                                string SQL = strQuery + " From " + strSQLTableName;
                                if (!string.IsNullOrEmpty(cmbDate.Value) && !string.IsNullOrEmpty(txtFromDate.Value) && !string.IsNullOrEmpty(txtToDate.Value) && !string.IsNullOrEmpty(cmbEmpID.Value))
                                {
                                    dateColumnName = cmbDate.Value;
                                    empidColumnName = cmbEmpID.Value.Trim();
                                    DateTime startDate = DateTime.ParseExact(txtFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                    DateTime EndDate = DateTime.ParseExact(txtToDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                   // WhereClause = " WHERE " + dateColumnName + " >= '" + startDate + "' AND " + dateColumnName + " <= '" + EndDate + "'";
                                   WhereClause = " WHERE " + dateColumnName + " >= '" + startDate.ToString("yyyy-MM-dd") + "' AND " + dateColumnName + " <= '" + EndDate.ToString("yyyy-MM-dd") + "' AND " + empidColumnName + " IN (" + ValidEmployeeList + ")";
                                   
                                }
                                if (!string.IsNullOrEmpty(WhereClause))
                                {
                                    SQL = SQL + WhereClause;
                                }
                                SqlCommand sqlCmdQuery = new SqlCommand();
                                sqlCmdQuery.CommandType = CommandType.Text;
                                sqlCmdQuery.CommandText = SQL;
                                sqlCmdQuery.Connection = sqlconn;
                                SqlDataAdapter sqlda = new SqlDataAdapter(sqlCmdQuery);
                                sqlda.Fill(dtTempRecord);
                            }
                        }
                        if (dtTempRecord != null && dtTempRecord.Rows.Count > 0)
                        {
                            int totalEmps = dtTempRecord.Rows.Count;
                            prog = oApplication.StatusBar.CreateProgressBar("Importing Employee(s) Attendance Record(s)", totalEmps, false);
                            prog.Value = 0;

                            DateTime startDate = DateTime.ParseExact(txtFromDate.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                            DateTime EndDate = DateTime.ParseExact(txtToDate.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                            var otempData = (from a in dbHrPayroll.TrnsTempAttendance
                                             where a.PunchedDate >= startDate
                                             && a.PunchedDate <= EndDate
                                             select a).ToList();
                            if (otempData.Count > 0)
                            {
                                dbHrPayroll.TrnsTempAttendance.DeleteAllOnSubmit(otempData);
                                dbHrPayroll.SubmitChanges();
                            }

                            for (int i = 0; i < dtTempRecord.Rows.Count; i++)
                            {
                                System.Windows.Forms.Application.DoEvents();
                                prog.Value += 1;

                                EmpCode = dtTempRecord.Rows[i][0].ToString();
                                InOut = dtTempRecord.Rows[i][1].ToString();
                                PunchDateTime = dtTempRecord.Rows[i][2].ToString();

                                if (string.IsNullOrEmpty(PunchDateTime))
                                {
                                    oApplication.StatusBar.SetText(EmpCode + " DateTime can't be found. Please Provide valid DateTime", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                    return;
                                }
                                var EmpRecord = dbHrPayroll.MstEmployee.Where(e => e.EmpID == EmpCode.Trim()).FirstOrDefault();
                                if (EmpRecord == null)
                                {
                                    oApplication.StatusBar.SetText(EmpCode + " EmpId can't be found. Please Provide valid Employee Code", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                    continue;
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(InOut))
                                    {
                                        oApplication.StatusBar.SetText("Please provide valid In Out code for Employee " + EmpCode, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                        return;
                                    }
                                    if (!string.IsNullOrEmpty(InOut))
                                    {
                                        InOut = InOut.Trim().ToUpper();
                                        switch (InOut)
                                        {
                                            case "01":
                                            case "1":
                                            case "02":
                                            case "2":
                                            case "I":
                                            case "U":
                                            case "O":
                                            case "i":
                                            case "u":
                                            case "o":
                                                break;
                                            default:
                                                oApplication.StatusBar.SetText("Please provide valid In Out code for Employee " + EmpCode + " for Date " + PunchDate, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                                return;
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(InOut) && InOut.Trim() == "01")
                                    {
                                        InOut = "1";
                                    }
                                    else if (!string.IsNullOrEmpty(InOut) && InOut.Trim() == "02")
                                    {
                                        InOut = "2";
                                    }
                                    else if (!string.IsNullOrEmpty(InOut) && (InOut.Trim() == "I" || InOut.Trim() == "i"))
                                    {
                                        InOut = "1";
                                    }
                                    else if (!string.IsNullOrEmpty(InOut) && (InOut.Trim() == "U" || InOut.Trim() == "O" || InOut.Trim() == "u" || InOut.Trim() == "o"))
                                    {
                                        InOut = "2";
                                    }

                                    DateTime dt = DateTime.Parse(PunchDateTime);
                                    string Time = dt.ToString("HH:mm");
                                    dt = dt.Date;
                                    TrnsTempAttendance TempAttdance = new TrnsTempAttendance();
                                    //TempAttdance.EmpID = EmpRecord.ID;
                                    TempAttdance.EmpID = EmpRecord.EmpID;
                                    TempAttdance.In_Out = InOut.Trim();
                                    TempAttdance.PunchedDate = dt;
                                    TempAttdance.PunchedTime = Time.Trim();
                                    TempAttdance.FlgProcessed = false;
                                    TempAttdance.CreatedDate = DateTime.Now;
                                    TempAttdance.UserID = oCompany.UserName;
                                    dbHrPayroll.TrnsTempAttendance.InsertOnSubmit(TempAttdance);
                                    dbHrPayroll.SubmitChanges();
                                }
                            }
                            
                            oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                            oApplication.StatusBar.SetText("Attendacne Imported Successfully", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                        }
                    }
                    catch (Exception Ex)
                    {
                        oApplication.StatusBar.SetText("ImportData Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    }
                    finally
                    {
                        if (prog != null)
                        {
                            prog.Stop();
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(prog);
                        }
                        prog = null;
                    }
                }
            }

            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("ImportData Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void ImportDataFromDBM3()
        {
            try
            {
                string dateColumnName = "";
                string WhereClause = "";
                string strFileLoc = txtFileLoc.Value;
                string strTableName = txtTblName.Value;
                string strServerName = txtSqlSN.Value;
                string strUserName = txtSqlUN.Value;
                string strPassword = txtSqlPas.Value;
                string strDBName = txtSqlDB.Value;
                string strSQLTableName = txtsqlTN.Value;
                SAPbouiCOM.ProgressBar prog = null;
                string empidColumnName = "";
                string strQuery = "Select ";
                string strSourceCollumnValue = "";
                for (int i = 1; i <= grdColumnMapp.RowCount; i++)
                {
                    strSourceCollumnValue = (grdColumnMapp.Columns.Item("cl_Sorc").Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                    if (!string.IsNullOrEmpty(strSourceCollumnValue))
                    {
                        dtColumnMap.SetValue("SourceColumn", i - 1, strSourceCollumnValue);
                    }
                    else
                    {
                        oApplication.StatusBar.SetText("Please Provide Valid Column(s) Mapping", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return;
                    }
                }
                for (int k = 0; k < dtColumnMap.Rows.Count; k++)
                {
                    strQuery = strQuery + Convert.ToString(dtColumnMap.GetValue("SourceColumn", k)) + ", ";
                }
                if (!string.IsNullOrEmpty(strQuery))
                {
                    try
                    {
                        strQuery = strQuery.Remove(strQuery.Length - 2, 2);
                        if (chkAccss.Checked && !string.IsNullOrEmpty(strFileLoc) && !string.IsNullOrEmpty(strTableName))
                        {
                            using (OleDbConnection Oledbconn = new OleDbConnection(String.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + strFileLoc + ";Persist Security Info=False")))
                            {
                                string SQL = strQuery + " From " + strTableName;
                                if (!string.IsNullOrEmpty(cmbDate.Value) && !string.IsNullOrEmpty(txtFromDate.Value) && !string.IsNullOrEmpty(txtToDate.Value) && !string.IsNullOrEmpty(cmbEmpID.Value))
                                {
                                    dateColumnName = cmbDate.Value;
                                    DateTime startDate = DateTime.ParseExact(txtFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                    DateTime EndDate = DateTime.ParseExact(txtToDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                    WhereClause = " WHERE " + dateColumnName + " >= '" + startDate + "' AND " + dateColumnName + " <= '" + EndDate + "'";
                                }
                                if (!string.IsNullOrEmpty(WhereClause))
                                {
                                    SQL = SQL + WhereClause;
                                }
                                OleDbCommand oledbcmdQuery = new OleDbCommand();
                                oledbcmdQuery.CommandType = CommandType.Text;
                                oledbcmdQuery.CommandText = SQL;
                                oledbcmdQuery.Connection = Oledbconn;
                                OleDbDataAdapter da = new OleDbDataAdapter(oledbcmdQuery);
                                da.Fill(dtTempRecord);
                            }
                        }
                        else if (chkSQL.Checked && !string.IsNullOrEmpty(strServerName) && !string.IsNullOrEmpty(strUserName) && !string.IsNullOrEmpty(strDBName) && !string.IsNullOrEmpty(strPassword))
                        {
                            using (SqlConnection sqlconn = new SqlConnection(String.Format("Server=" + strServerName + ";Database=" + strDBName + ";User Id=" + strUserName + ";Password=" + strPassword + ";")))
                            {
                                string SQL = strQuery + " From " + strSQLTableName;
                                if (!string.IsNullOrEmpty(cmbDate.Value) && !string.IsNullOrEmpty(txtFromDate.Value) && !string.IsNullOrEmpty(txtToDate.Value) && !string.IsNullOrEmpty(cmbEmpID.Value))
                                {
                                    dateColumnName = cmbDate.Value;
                                    empidColumnName = cmbEmpID.Value.Trim();
                                    DateTime startDate = DateTime.ParseExact(txtFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                    DateTime EndDate = DateTime.ParseExact(txtToDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                    //WhereClause = " WHERE " + dateColumnName + " >= '" + startDate + "' AND " + dateColumnName + " <= '" + EndDate + "'";
                                    WhereClause = " WHERE " + dateColumnName + " >= '" + startDate.ToString("yyyy-MM-dd") + "' AND " + dateColumnName + " <= '" + EndDate.ToString("yyyy-MM-dd") + "' AND " + empidColumnName + " IN (" + ValidEmployeeList + ")";
                                }
                                if (!string.IsNullOrEmpty(WhereClause))
                                {
                                    SQL = SQL + WhereClause;
                                }
                                SqlCommand sqlCmdQuery = new SqlCommand();
                                sqlCmdQuery.CommandType = CommandType.Text;
                                sqlCmdQuery.CommandText = SQL;
                                sqlCmdQuery.Connection = sqlconn;
                                SqlDataAdapter sqlda = new SqlDataAdapter(sqlCmdQuery);
                                sqlda.Fill(dtTempRecord);
                            }
                        }
                        if (dtTempRecord != null && dtTempRecord.Rows.Count > 0)
                        {
                            int totalEmps = dtTempRecord.Rows.Count;
                            prog = oApplication.StatusBar.CreateProgressBar("Importing Employee(s) Attendance Record(s)", totalEmps, false);
                            prog.Value = 0;

                            DateTime startDate = DateTime.ParseExact(txtFromDate.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                            DateTime EndDate = DateTime.ParseExact(txtToDate.Value.Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                            var otempData = (from a in dbHrPayroll.TrnsTempAttendance
                                             where a.PunchedDate >= startDate
                                             && a.PunchedDate <= EndDate
                                             select a).ToList();
                            if (otempData.Count > 0)
                            {
                                dbHrPayroll.TrnsTempAttendance.DeleteAllOnSubmit(otempData);
                                dbHrPayroll.SubmitChanges();
                            }

                            for (int i = 0; i < dtTempRecord.Rows.Count; i++)
                            {
                                System.Windows.Forms.Application.DoEvents();
                                prog.Value += 1;

                                EmpCode = dtTempRecord.Rows[i][0].ToString();
                                DateIn = dtTempRecord.Rows[i][1].ToString().Trim();
                                TimeIn = dtTempRecord.Rows[i][2].ToString().Trim();
                                if (!string.IsNullOrEmpty(TimeIn) && TimeIn.Length > 12)
                                {
                                    DateTime dt = DateTime.Parse(TimeIn);
                                    TimeIn = dt.ToString("HH:mm");
                                }
                                else
                                {
                                    TimeSpan ts = TimeSpan.Parse("00:00");
                                    TimeIn = ts.ToString(@"hh\:mm");
                                }
                                DateOut = dtTempRecord.Rows[i][3].ToString().Trim();
                                TimeOut = dtTempRecord.Rows[i][4].ToString().Trim();
                                if (!string.IsNullOrEmpty(TimeOut) && TimeOut.Length > 12)
                                {
                                    DateTime dtOut = DateTime.Parse(TimeOut);
                                    TimeOut = dtOut.ToString("HH:mm");
                                }
                                else
                                {
                                    TimeSpan ts = TimeSpan.Parse("00:00");
                                    TimeOut = ts.ToString(@"hh\:mm");
                                }

                                var EmpRecord = dbHrPayroll.MstEmployee.Where(e => e.EmpID == EmpCode.Trim()).FirstOrDefault();
                                if (EmpRecord == null)
                                {
                                    oApplication.StatusBar.SetText(EmpCode + " EmpId can't be found. Please Provide valid Employee Code", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                    continue;
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(DateIn) && !string.IsNullOrEmpty(TimeIn))
                                    {
                                       
                                        //TimeSpan ts = TimeSpan.Parse(TimeIn);
                                        //TimeIn = ts.ToString(@"hh\:mm");
                                        //TimeIn = string.Format("{0:t}", TimeIn);                                      
                                        TrnsTempAttendance TempAttdance = new TrnsTempAttendance();
                                        //TempAttdance.EmpID = EmpRecord.ID;
                                        TempAttdance.EmpID = EmpRecord.EmpID;
                                        TempAttdance.PunchedDate = Convert.ToDateTime(DateIn).Date;
                                        TempAttdance.PunchedTime = TimeIn.Trim();
                                        TempAttdance.FlgProcessed = false;
                                        TempAttdance.In_Out = "1";
                                        TempAttdance.CreatedDate = DateTime.Now;
                                        //TempAttdance.UserID = oCompany.UserName;
                                        dbHrPayroll.TrnsTempAttendance.InsertOnSubmit(TempAttdance);
                                    }
                                    if (!string.IsNullOrEmpty(DateOut) && !string.IsNullOrEmpty(TimeOut))
                                    {
                                       
                                        //TimeSpan ts = TimeSpan.Parse(TimeOut);
                                        //TimeOut = ts.ToString(@"hh\:mm");
                                        TrnsTempAttendance TempAttdance2 = new TrnsTempAttendance();
                                        //TempAttdance2.EmpID = EmpRecord.ID;
                                        TempAttdance2.EmpID = EmpRecord.EmpID;
                                        TempAttdance2.PunchedDate = Convert.ToDateTime(DateOut).Date;
                                        TempAttdance2.PunchedTime = TimeOut.Trim();
                                        TempAttdance2.FlgProcessed = false;
                                        TempAttdance2.In_Out = "2";
                                        TempAttdance2.CreatedDate = DateTime.Now;
                                        //TempAttdance2.UserID = oCompany.UserName;
                                        dbHrPayroll.TrnsTempAttendance.InsertOnSubmit(TempAttdance2);
                                    }
                                }
                            }
                            dbHrPayroll.SubmitChanges();
                            oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                            oApplication.StatusBar.SetText("Attendacne Imported Successfully", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                        }
                    }
                    catch (Exception Ex)
                    {
                        oApplication.StatusBar.SetText("ImportData Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    }
                    finally
                    {
                        if (prog != null)
                        {
                            prog.Stop();
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(prog);
                        }
                        prog = null;
                    }
                }
            }

            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("ImportData Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void ImportDataFromDBM4()
        {
            try
            {
                string dateColumnName = "";
                string WhereClause = "";
                string strFileLoc = txtFileLoc.Value;
                string strTableName = txtTblName.Value;
                string strServerName = txtSqlSN.Value;
                string strUserName = txtSqlUN.Value;
                string strPassword = txtSqlPas.Value;
                string strDBName = txtSqlDB.Value;
                string strSQLTableName = txtsqlTN.Value;
                SAPbouiCOM.ProgressBar prog = null;

                string strQuery = "Select ";
                string strSourceCollumnValue = "";
                for (int i = 1; i <= grdColumnMapp.RowCount; i++)
                {
                    strSourceCollumnValue = (grdColumnMapp.Columns.Item("cl_Sorc").Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                    if (!string.IsNullOrEmpty(strSourceCollumnValue))
                    {
                        dtColumnMap.SetValue("SourceColumn", i - 1, strSourceCollumnValue);
                    }
                    else
                    {
                        oApplication.StatusBar.SetText("Please Provide Valid Column(s) Mapping", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return;
                    }
                }
                for (int k = 0; k < dtColumnMap.Rows.Count; k++)
                {
                    strQuery = strQuery + Convert.ToString(dtColumnMap.GetValue("SourceColumn", k)) + ", ";
                }
                if (!string.IsNullOrEmpty(strQuery))
                {
                    try
                    {
                        strQuery = strQuery.Remove(strQuery.Length - 2, 2);
                        if (chkAccss.Checked && !string.IsNullOrEmpty(strFileLoc) && !string.IsNullOrEmpty(strTableName))
                        {
                            using (OleDbConnection Oledbconn = new OleDbConnection(String.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + strFileLoc + ";Persist Security Info=False")))
                            {
                                string SQL = strQuery + " From " + strTableName;
                                if (!string.IsNullOrEmpty(cmbDate.Value) && !string.IsNullOrEmpty(txtFromDate.Value) && !string.IsNullOrEmpty(txtToDate.Value) && !string.IsNullOrEmpty(cmbEmpID.Value))
                                {
                                    dateColumnName = cmbDate.Value;
                                    DateTime startDate = DateTime.ParseExact(txtFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                    DateTime EndDate = DateTime.ParseExact(txtToDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                    WhereClause = " WHERE " + dateColumnName + " >= '" + startDate + "' AND " + dateColumnName + " <= '" + EndDate + "'";
                                }
                                if (!string.IsNullOrEmpty(WhereClause))
                                {
                                    SQL = SQL + WhereClause;
                                }
                                OleDbCommand oledbcmdQuery = new OleDbCommand();
                                oledbcmdQuery.CommandType = CommandType.Text;
                                oledbcmdQuery.CommandText = SQL;
                                oledbcmdQuery.Connection = Oledbconn;
                                OleDbDataAdapter da = new OleDbDataAdapter(oledbcmdQuery);
                                da.Fill(dtTempRecord);
                            }
                        }
                        else if (chkSQL.Checked && !string.IsNullOrEmpty(strServerName) && !string.IsNullOrEmpty(strUserName) && !string.IsNullOrEmpty(strDBName) && !string.IsNullOrEmpty(strPassword))
                        {
                            using (SqlConnection sqlconn = new SqlConnection(String.Format("Server=" + strServerName + ";Database=" + strDBName + ";User Id=" + strUserName + ";Password=" + strPassword + ";")))
                            {
                                string SQL = strQuery + " From " + strSQLTableName;
                                if (!string.IsNullOrEmpty(cmbDate.Value) && !string.IsNullOrEmpty(txtFromDate.Value) && !string.IsNullOrEmpty(txtToDate.Value) && !string.IsNullOrEmpty(cmbEmpID.Value))
                                {
                                    dateColumnName = cmbDate.Value;
                                    DateTime startDate = DateTime.ParseExact(txtFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                    DateTime EndDate = DateTime.ParseExact(txtToDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                    WhereClause = " WHERE " + dateColumnName + " >= '" + startDate + "' AND " + dateColumnName + " <= '" + EndDate + "'";
                                }
                                if (!string.IsNullOrEmpty(WhereClause))
                                {
                                    SQL = SQL + WhereClause;
                                }
                                SqlCommand sqlCmdQuery = new SqlCommand();
                                sqlCmdQuery.CommandType = CommandType.Text;
                                sqlCmdQuery.CommandText = SQL;
                                sqlCmdQuery.Connection = sqlconn;
                                SqlDataAdapter sqlda = new SqlDataAdapter(sqlCmdQuery);
                                sqlda.Fill(dtTempRecord);
                            }
                        }
                        if (dtTempRecord != null && dtTempRecord.Rows.Count > 0)
                        {
                            int totalEmps = dtTempRecord.Rows.Count;
                            prog = oApplication.StatusBar.CreateProgressBar("Importing Employee(s) Attendance Record(s)", totalEmps, false);
                            prog.Value = 0;

                            for (int i = 0; i < dtTempRecord.Rows.Count; i++)
                            {
                                System.Windows.Forms.Application.DoEvents();
                                prog.Value += 1;

                                EmpCode = dtTempRecord.Rows[i][0].ToString();
                                DateTimeIn = dtTempRecord.Rows[i][1].ToString().Trim();
                                DateTimeOut = dtTempRecord.Rows[i][2].ToString().Trim();

                                var EmpRecord = dbHrPayroll.MstEmployee.Where(e => e.EmpID == EmpCode.Trim()).FirstOrDefault();
                                if (EmpRecord == null)
                                {
                                    oApplication.StatusBar.SetText(EmpCode + " EmpId can't be found. Please Provide valid Employee Code", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                    continue;
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(DateTimeIn))
                                    {
                                        //Duplicate Attendance Check
                                        int tempcheck = 0;
                                        tempcheck = (from a in dbHrPayroll.TrnsTempAttendance where a.EmpID == EmpCode & a.In_Out == "1" & a.PunchedDate == Convert.ToDateTime(DateTimeIn) select a).Count();
                                        if (tempcheck > 0)
                                        {
                                            var OldTempRecords = (from a in dbHrPayroll.TrnsTempAttendance where a.EmpID == EmpCode & a.In_Out == "1" & a.PunchedDate == Convert.ToDateTime(DateTimeIn) select a).ToList();
                                            if (OldTempRecords.Count > 0)
                                            {
                                                foreach (var oneline in OldTempRecords)
                                                {
                                                    dbHrPayroll.TrnsTempAttendance.DeleteOnSubmit(oneline);
                                                }
                                                dbHrPayroll.SubmitChanges();
                                            }
                                        }
                                        //

                                        TimeIn = string.Empty;
                                        DateTime dtIn = DateTime.Parse(DateTimeIn);
                                        TimeIn = dtIn.ToString("HH:mm");
                                        TrnsTempAttendance TempAttdance = new TrnsTempAttendance();
                                        //TempAttdance.EmpID = EmpRecord.ID;
                                        TempAttdance.EmpID = EmpRecord.EmpID;
                                        TempAttdance.PunchedDate = dtIn.Date;
                                        TempAttdance.PunchedTime = TimeIn.Trim();
                                        TempAttdance.In_Out = "1";
                                        TempAttdance.CreatedDate = DateTime.Now;
                                        TempAttdance.UserID = oCompany.UserName;
                                        dbHrPayroll.TrnsTempAttendance.InsertOnSubmit(TempAttdance);
                                    }
                                    if (!string.IsNullOrEmpty(DateTimeOut))
                                    {
                                        //Duplicate Attendance Check
                                        int tempcheck = 0;
                                        tempcheck = (from a in dbHrPayroll.TrnsTempAttendance where a.EmpID == EmpCode & a.In_Out == "2" & a.PunchedDate == Convert.ToDateTime(DateTimeOut) select a).Count();
                                        if (tempcheck > 0)
                                        {
                                            var OldTempRecords = (from a in dbHrPayroll.TrnsTempAttendance where a.EmpID == EmpCode & a.In_Out == "2" & a.PunchedDate == Convert.ToDateTime(DateTimeOut) select a).ToList();
                                            if (OldTempRecords.Count > 0)
                                            {
                                                foreach (var oneline in OldTempRecords)
                                                {
                                                    dbHrPayroll.TrnsTempAttendance.DeleteOnSubmit(oneline);
                                                }
                                                dbHrPayroll.SubmitChanges();
                                            }
                                        }
                                        //

                                        TimeOut = string.Empty;
                                        DateTime dtOut = DateTime.Parse(DateTimeOut);
                                        TimeOut = dtOut.ToString("HH:mm");
                                        TrnsTempAttendance TempAttdance2 = new TrnsTempAttendance();
                                        //TempAttdance2.EmpID = EmpRecord.ID;
                                        TempAttdance2.EmpID = EmpRecord.EmpID;
                                        TempAttdance2.PunchedDate = dtOut.Date;
                                        TempAttdance2.PunchedTime = TimeOut.Trim();
                                        TempAttdance2.In_Out = "2";
                                        TempAttdance2.CreatedDate = DateTime.Now;
                                        TempAttdance2.UserID = oCompany.UserName;
                                        dbHrPayroll.TrnsTempAttendance.InsertOnSubmit(TempAttdance2);
                                    }
                                }
                            }
                            dbHrPayroll.SubmitChanges();
                            oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                            oApplication.StatusBar.SetText("Attendacne Imported Successfully", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                        }
                    }
                    catch (Exception Ex)
                    {
                        oApplication.StatusBar.SetText("ImportData Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    }
                    finally
                    {
                        if (prog != null)
                        {
                            prog.Stop();
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(prog);
                        }
                        prog = null;
                    }
                }
            }

            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("ImportData Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void ImportDataFromDBM5()
        {
            try
            {
                string strFileLoc = txtFileLoc.Value;
                string strTableName = txtTblName.Value;
                string strServerName = txtSqlSN.Value;
                string strUserName = txtSqlUN.Value;
                string strPassword = txtSqlPas.Value;
                string strDBName = txtSqlDB.Value;
                string strSQLTableName = txtsqlTN.Value;
                string strSourceCollumnField = "InOut";
                string strSourceCollumnValue = "";
                string dateColumnName = "";
                string WhereClause = "";
                string WhereTypes = " ";
                string TypeColumnsName = "";
                string valuesSearchIn = "";
                List<string> InTypes = new List<string>();
                List<string> OutTypes = new List<string>();
                var oSettings = from a in dbHrPayroll.CfgAttandanceSettings where a.MachineType == "M5" select a;

                int count = oSettings.Count();
                int tt = 0;
                foreach (var Record in oSettings)
                {
                    if ((Record.TimeType.ToUpper() == "IN") || (Record.TimeType.ToUpper() == "OUT"))
                    {
                        valuesSearchIn += " '" + Record.TimeValue.Trim() + "'";
                    }
                    tt++;
                    if (tt != count)
                    {
                        valuesSearchIn += ",";
                    }

                }


                SAPbouiCOM.ProgressBar prog = null;

                string strQuery = "Select ";

                for (int i = 1; i <= grdColumnMapp.RowCount; i++)
                {
                    strSourceCollumnField = (grdColumnMapp.Columns.Item("cl_dest").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                    strSourceCollumnValue = (grdColumnMapp.Columns.Item("cl_Sorc").Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                    if (!string.IsNullOrEmpty(strSourceCollumnValue))
                    {
                        dtColumnMap.SetValue("SourceColumn", i - 1, strSourceCollumnValue);
                        if (strSourceCollumnField == "InOut")
                        {
                            TypeColumnsName = strSourceCollumnValue;
                        }
                    }
                    else
                    {
                        oApplication.StatusBar.SetText("Please Provide Valid Column(s) Mapping", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return;
                    }
                }
                for (int k = 0; k < dtColumnMap.Rows.Count; k++)
                {
                    strQuery = strQuery + Convert.ToString(dtColumnMap.GetValue("SourceColumn", k)) + ", ";
                }
                if (!string.IsNullOrEmpty(strQuery))
                {
                    try
                    {
                        strQuery = strQuery.Remove(strQuery.Length - 2, 2);
                        if (chkAccss.Checked && !string.IsNullOrEmpty(strFileLoc) && !string.IsNullOrEmpty(strTableName))
                        {
                            using (OleDbConnection Oledbconn = new OleDbConnection(String.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + strFileLoc + ";Persist Security Info=False")))
                            {
                                string SQL = strQuery + " From " + strTableName;
                                if (!string.IsNullOrEmpty(cmbDate.Value) && !string.IsNullOrEmpty(txtFromDate.Value) && !string.IsNullOrEmpty(txtToDate.Value) && !string.IsNullOrEmpty(cmbEmpID.Value))
                                {
                                    dateColumnName = cmbDate.Value;
                                    DateTime startDate = DateTime.ParseExact(txtFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                    DateTime EndDate = DateTime.ParseExact(txtToDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                    WhereClause = " WHERE " + dateColumnName + " >= '" + startDate + "' AND " + dateColumnName + " <= '" + EndDate + "'";
                                }

                                if (true)
                                {
                                    WhereTypes += " And " + TypeColumnsName + " In (" + valuesSearchIn + ")";
                                }

                                if (!string.IsNullOrEmpty(WhereClause))
                                {
                                    SQL = SQL + WhereClause + WhereTypes;
                                }
                                OleDbCommand oledbcmdQuery = new OleDbCommand();
                                oledbcmdQuery.CommandType = CommandType.Text;
                                oledbcmdQuery.CommandText = SQL;
                                oledbcmdQuery.Connection = Oledbconn;
                                OleDbDataAdapter da = new OleDbDataAdapter(oledbcmdQuery);
                                da.Fill(dtTempRecord);
                            }
                        }
                        else if (chkSQL.Checked && !string.IsNullOrEmpty(strServerName) && !string.IsNullOrEmpty(strUserName) && !string.IsNullOrEmpty(strDBName) && !string.IsNullOrEmpty(strPassword))
                        {
                            using (SqlConnection sqlconn = new SqlConnection(String.Format("Server=" + strServerName + ";Database=" + strDBName + ";User Id=" + strUserName + ";Password=" + strPassword + ";")))
                            {
                                string SQL = strQuery + " From " + strSQLTableName;
                                if (!string.IsNullOrEmpty(cmbDate.Value) && !string.IsNullOrEmpty(txtFromDate.Value) && !string.IsNullOrEmpty(txtToDate.Value) && !string.IsNullOrEmpty(cmbEmpID.Value))
                                {
                                    dateColumnName = cmbDate.Value;
                                    DateTime startDate = DateTime.ParseExact(txtFromDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                    DateTime EndDate = DateTime.ParseExact(txtToDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                    WhereClause = " WHERE " + dateColumnName + " >= '" + startDate + "' AND " + dateColumnName + " <= '" + EndDate + "'";
                                }

                                if (true)
                                {
                                    WhereTypes += " And " + TypeColumnsName + " In (" + valuesSearchIn + ")";
                                }

                                if (!string.IsNullOrEmpty(WhereClause))
                                {
                                    SQL = SQL + WhereClause + WhereTypes;
                                }
                                SqlCommand sqlCmdQuery = new SqlCommand();
                                sqlCmdQuery.CommandType = CommandType.Text;
                                sqlCmdQuery.CommandText = SQL;
                                sqlCmdQuery.Connection = sqlconn;
                                SqlDataAdapter sqlda = new SqlDataAdapter(sqlCmdQuery);
                                sqlda.Fill(dtTempRecord);
                            }
                        }
                        if (dtTempRecord != null && dtTempRecord.Rows.Count > 0)
                        {
                            int totalEmps = dtTempRecord.Rows.Count;
                            prog = oApplication.StatusBar.CreateProgressBar("Importing Employee(s) Attendance Record(s)", totalEmps, false);
                            prog.Value = 0;

                            for (int i = 0; i < dtTempRecord.Rows.Count; i++)
                            {
                                System.Windows.Forms.Application.DoEvents();
                                prog.Value += 1;

                                EmpCode = dtTempRecord.Rows[i][0].ToString();
                                InOut = dtTempRecord.Rows[i][1].ToString();
                                PunchDate = dtTempRecord.Rows[i][2].ToString();
                                PunchTime = dtTempRecord.Rows[i][3].ToString();
                                CostCenter = dtTempRecord.Rows[i][4].ToString();

                                if (string.IsNullOrEmpty(PunchTime))
                                {
                                    oApplication.StatusBar.SetText(EmpCode + " PunchTime can't be found. Please Provide valid PunchTime", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                    continue;
                                }
                                if (string.IsNullOrEmpty(PunchDate))
                                {
                                    oApplication.StatusBar.SetText(EmpCode + " Date can't be found. Please Provide valid Date", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                    continue;
                                }
                                if (!string.IsNullOrEmpty(PunchTime) && PunchTime.Length > 12)
                                {
                                    DateTime dt = DateTime.Parse(PunchTime);
                                    TimeIn = dt.ToString("HH:mm");
                                }
                                //Attendance Duplication
                                int chkTempAttandance = 0;
                                chkTempAttandance = (from a in dbHrPayroll.TrnsTempAttendance
                                                     where a.EmpID == EmpCode
                                                     select a).Count();
                                if (chkTempAttandance > 0)
                                {
                                    //var oCollection = (from a in dbHrPayroll.TrnsTempAttendance
                                    //                   where a.EmpID == EmpCode
                                    //                   select a).ToList();
                                    var oCollection = (from a in dbHrPayroll.TrnsTempAttendance
                                                       where a.EmpID == EmpCode && a.PunchedDate == Convert.ToDateTime(PunchDate).Date
                                                       && a.PunchedTime == TimeIn
                                                       select a).ToList();
                                    foreach (var One in oCollection)
                                    {
                                        //dbHrPayroll.TrnsTempAttendance.DeleteOnSubmit(One);
                                        continue;
                                    }
                                    dbHrPayroll.SubmitChanges();
                                }
                                //End Attendance Duplication

                                
                                var EmpRecord = dbHrPayroll.MstEmployee.Where(e => e.EmpID == EmpCode.Trim()).FirstOrDefault();
                                if (EmpRecord == null)
                                {
                                    oApplication.StatusBar.SetText(EmpCode + " EmpId can't be found. Please Provide valid Employee Code", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    //return;
                                    continue;
                                    //TODO: Behtareen kaam now attendance of employee not exist in db also 
                                }
                                else
                                {
                                    //if (!EmpRecord.FlgActive.Value)
                                    //{
                                    //    oApplication.StatusBar.SetText("Employee is deactive with EmpCode " + EmpCode, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                    //    return;
                                    //}
                                    if (string.IsNullOrEmpty(InOut))
                                    {
                                        oApplication.StatusBar.SetText("Please provide valid In Out code for Employee " + EmpCode, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                        continue;
                                    }
                                    if (!string.IsNullOrEmpty(InOut))
                                    {
                                        InOut = InOut.Trim().ToUpper();
                                        switch (InOut)
                                        {
                                            case "IN":
                                            case "OUT":
                                            case "CLOCK-IN":
                                            case "CLOCK-OUT":
                                                break;
                                            default:
                                                oApplication.StatusBar.SetText("Please provide valid In Out code for Employee " + EmpCode + " for Date " + PunchDate, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                                continue;
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(InOut) && (InOut.Trim() == "IN" || InOut.Trim() == "CLOCK-IN"))
                                    {
                                        InOut = "1";
                                    }
                                    else if (!string.IsNullOrEmpty(InOut) && (InOut.Trim() == "OUT" || InOut.Trim() == "CLOCK-OUT"))
                                    {
                                        InOut = "2";
                                    }

                                    TimeSpan ts = TimeSpan.Parse(TimeIn);
                                    PunchTime = ts.ToString(@"hh\:mm");
                                    TrnsTempAttendance TempAttdance = new TrnsTempAttendance();
                                    //TempAttdance.EmpID = EmpRecord.ID;
                                    TempAttdance.EmpID = EmpRecord.EmpID;
                                    TempAttdance.In_Out = InOut.Trim();
                                    TempAttdance.PunchedDate = Convert.ToDateTime(PunchDate).Date;
                                    TempAttdance.PunchedTime = PunchTime.Trim();
                                    TempAttdance.CostCenter = CostCenter.Trim();
                                    TempAttdance.CreatedDate = DateTime.Now;
                                    TempAttdance.UserID = oCompany.UserName;
                                    dbHrPayroll.TrnsTempAttendance.InsertOnSubmit(TempAttdance);
                                    dbHrPayroll.SubmitChanges();
                                }
                            }
                            //dbHrPayroll.SubmitChanges();
                            oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                            oApplication.StatusBar.SetText("Attendacne Imported Successfully", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                        }
                        else
                        {
                            oApplication.StatusBar.SetText("No Record Found", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_None);
                        }
                    }
                    catch (Exception Ex)
                    {
                        oApplication.StatusBar.SetText("ImportData Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    }
                    finally
                    {
                        if (prog != null)
                        {
                            prog.Stop();
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(prog);
                        }
                        prog = null;
                    }
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("ImportDataFromDBM5 Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void TestMSACCESSConnection()
        {

            string strFileLoc = txtFileLoc.Value;
            string strTableName = txtTblName.Value;
            if (!string.IsNullOrEmpty(strFileLoc) && !string.IsNullOrEmpty(strTableName))
            {
                using (OleDbConnection conn = new OleDbConnection(String.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + strFileLoc + ";Persist Security Info=False")))
                {

                    try
                    {
                        conn.Open();
                        oApplication.MessageBox(Program.objHrmsUI.getStrMsg("ConnectionSuccessfull"));
                    }
                    catch (Exception Ex)
                    {
                        oApplication.MessageBox(Program.objHrmsUI.getStrMsg("ConnectionFailed"));
                        oApplication.StatusBar.SetText("ImportData Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            else
            {
                oApplication.StatusBar.SetText("Please Provide valid Information to Test Connection.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void TestSQLConnection()
        {

            string strServerName = txtSqlSN.Value;
            string strUserName = txtSqlUN.Value;
            string strPassword = txtSqlPas.Value;
            string strDBName = txtSqlDB.Value;
            if (!string.IsNullOrEmpty(strServerName) && !string.IsNullOrEmpty(strUserName) && !string.IsNullOrEmpty(strPassword) && !string.IsNullOrEmpty(strDBName))
            {
                using (SqlConnection conn = new SqlConnection(String.Format("Server=" + strServerName + ";Database=" + strDBName + ";User Id=" + strUserName + ";Password=" + strPassword + ";")))
                {

                    try
                    {
                        conn.Open();
                        oApplication.MessageBox(Program.objHrmsUI.getStrMsg("ConnectionSuccessfull"));
                    }
                    catch (Exception Ex)
                    {
                        oApplication.MessageBox(Program.objHrmsUI.getStrMsg("ConnectionFailed"));
                        oApplication.StatusBar.SetText("ImportData Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            else
            {
                oApplication.StatusBar.SetText("Please Provide valid Information to Test Connection.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void ActivateAccessFields()
        {
            try
            {
                if (chkAccss.Checked)
                {                    
                    ItxtFileLoc.Enabled = true;
                    txtFileLoc.Active = true;
                    ItxtTblName.Enabled = true;                    
                    ItxtUserName.Enabled = true;                  
                    ItxtPassword.Enabled = true;
                    chkSQL.Checked = false;                    
                    ItxtSqlSN.Enabled = false;
                    ItxtSqlUN.Enabled = false;
                    ItxtSqlPas.Enabled = false;
                    ItxtSqlDB.Enabled = false;
                    ItxtsqlTN.Enabled = false;

                }               
            }
            catch (Exception Ex)
            {
               oApplication.StatusBar.SetText("ActivateAccessFields Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void ActivateSQLFields()
        {
            try
            {
                if (chkSQL.Checked)
                {               
                    ItxtSqlSN.Enabled = true;
                    txtSqlSN.Active = true;
                    ItxtSqlUN.Enabled = true;
                    ItxtSqlPas.Enabled = true;
                    ItxtSqlDB.Enabled = true;
                    ItxtsqlTN.Enabled = true;
                    chkAccss.Checked = false;
                    ItxtFileLoc.Enabled = false;
                    ItxtTblName.Enabled = false;
                    ItxtUserName.Enabled = false;
                    ItxtPassword.Enabled = false;                
                }               
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("ActivateSQLFields Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void SaveRecords()
        {
            try
            {
                if (chkSQL.Checked)
                {
                    if (!string.IsNullOrEmpty(txtSqlSN.Value) && !string.IsNullOrEmpty(txtSqlUN.Value) && !string.IsNullOrEmpty(txtSqlPas.Value) && !string.IsNullOrEmpty(txtSqlDB.Value) && !string.IsNullOrEmpty(txtsqlTN.Value))
                    {
                        int RecordCount = dbHrPayroll.CfgConnectionSetUp.Count();
                        if (RecordCount > 0)
                        {
                            var objConnSetUpX = dbHrPayroll.CfgConnectionSetUp.FirstOrDefault();
                            if (objConnSetUpX != null)
                            {
                                objConnSetUpX.FlgSqlServer = true;
                                objConnSetUpX.FlgAccess = false;
                                objConnSetUpX.ServerName = txtSqlSN.Value;
                                objConnSetUpX.UserName = txtSqlUN.Value;
                                objConnSetUpX.Password = txtSqlPas.Value;
                                objConnSetUpX.DBName = txtSqlDB.Value;
                                objConnSetUpX.TableName = txtsqlTN.Value;
                            }
                        }
                        else
                        {
                            CfgConnectionSetUp objConnSetUp = new CfgConnectionSetUp();
                            objConnSetUp.FlgSqlServer = true;
                            objConnSetUp.FlgAccess = false;
                            objConnSetUp.ServerName = txtSqlSN.Value;
                            objConnSetUp.UserName = txtSqlUN.Value;
                            objConnSetUp.Password = txtSqlPas.Value;
                            objConnSetUp.DBName = txtSqlDB.Value;
                            objConnSetUp.TableName = txtsqlTN.Value;
                            dbHrPayroll.CfgConnectionSetUp.InsertOnSubmit(objConnSetUp);
                        }
                        dbHrPayroll.SubmitChanges();
                    }
                    else
                    {
                        oApplication.MessageBox("Required Field Missing");
                    }
                }
                else if (chkAccss.Checked)
                {
                    if (!string.IsNullOrEmpty(txtFileLoc.Value) && !string.IsNullOrEmpty(txtTblName.Value))
                    {
                        int RecordCount = dbHrPayroll.CfgConnectionSetUp.Count();
                        if (RecordCount > 0)
                        {
                            var objConnSetUpX = dbHrPayroll.CfgConnectionSetUp.FirstOrDefault();
                            if (objConnSetUpX != null)
                            {
                                objConnSetUpX.FlgSqlServer = false;
                                objConnSetUpX.FlgAccess = true;
                                objConnSetUpX.FileLocation = txtFileLoc.Value;
                                objConnSetUpX.TableName = txtTblName.Value;
                                if (!string.IsNullOrEmpty(txtUserName.Value))
                                {
                                    objConnSetUpX.UserName = txtUserName.Value;
                                }
                                if (!string.IsNullOrEmpty(txtPassword.Value))
                                {
                                    objConnSetUpX.Password = txtPassword.Value;
                                }              
                            }
                        }
                        else
                        {
                            CfgConnectionSetUp objConnSetUp = new CfgConnectionSetUp();
                            objConnSetUp.FlgSqlServer = false;
                            objConnSetUp.FlgAccess = true;
                            objConnSetUp.FileLocation = txtFileLoc.Value;
                            objConnSetUp.TableName = txtTblName.Value;
                            if (!string.IsNullOrEmpty(txtUserName.Value))
                            {
                                objConnSetUp.UserName = txtUserName.Value;
                            }
                            if (!string.IsNullOrEmpty(txtPassword.Value))
                            {
                                objConnSetUp.Password = txtPassword.Value;
                            }                                                       
                            dbHrPayroll.CfgConnectionSetUp.InsertOnSubmit(objConnSetUp);
                        }
                        dbHrPayroll.SubmitChanges();
                    }
                }
                else
                {
                    oApplication.MessageBox("Please select Data Base Type");
                }
            }
            catch (Exception Ex)
            {
               oApplication.StatusBar.SetText("SaveRecords Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }       
        
        private void GetSaveRecords()
        {
            try
            {
                int RecordCount = dbHrPayroll.CfgConnectionSetUp.Count();
                if (RecordCount > 0)
                {
                    var objConnSetUpX = dbHrPayroll.CfgConnectionSetUp.FirstOrDefault();
                    if (objConnSetUpX.FlgSqlServer == true)
                    {
                        chkSQL.Checked = true;
                        chkAccss.Checked = false;
                        txtSqlSN.Value = objConnSetUpX.ServerName;
                        txtSqlUN.Value = objConnSetUpX.UserName;
                        txtSqlPas.Value = objConnSetUpX.Password;
                        txtSqlDB.Value = objConnSetUpX.DBName;
                        txtsqlTN.Value = objConnSetUpX.TableName;
                        PopulateSourceColumnComboFROMSQLSERVER();
                    }
                    else if (objConnSetUpX.FlgAccess == true)
                    {
                        chkSQL.Checked = false;
                        chkAccss.Checked = true;
                        txtFileLoc.Value = objConnSetUpX.FileLocation;
                        txtTblName.Value = objConnSetUpX.TableName;
                        txtUserName.Value = objConnSetUpX.UserName;
                        txtPassword.Value = objConnSetUpX.Password;
                        PopulateSourceColumnComboFROMMSACCESS();
                    }
               
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("SaveRecords Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }        
      
        private void picMachineType()
        {
            string strSql = sqlString.getSql("attMachines", SearchKeyVal);
            picker pic = new picker(oApplication, ds.getDataTable(strSql));
            System.Data.DataTable st = pic.ShowInput("Select Machine", "Select Machine for Record Fetching");
            pic = null;
            if (st.Rows.Count > 0)
            {
                txtMachineType.Value = st.Rows[0][0].ToString();
                if (!string.IsNullOrEmpty(txtMachineType.Value))
                {
                    string MacType = txtMachineType.Value;
                    switch (txtMachineType.Value)
                    {
                        case "M1":
                            FillColumnMappingGridM1();
                            break;
                        case "M2":
                            FillColumnMappingGridM2();
                            break;
                        case "M3":
                            FillColumnMappingGridM3();
                            break;
                        case "M4":
                            FillColumnMappingGridM4();
                            break;
                        case "M5":
                            FillColumnMappingGridM5();
                            break;
                    }
                }                
            }
        }
        
        private void ImportDataFromMachine()
        {
            if (!string.IsNullOrEmpty(txtMachineType.Value))
            {
                string MacType = txtMachineType.Value;
                switch (txtMachineType.Value)
                {
                    case "M1":
                        ImportDataFromDBM1();
                        break;
                    case "M2":                        
                        ImportDataFromDBM2();
                        break;
                    case "M3":
                        ImportDataFromDBM3();
                        break;
                    case "M4":
                        ImportDataFromDBM4();
                        break;
                    case "M5":
                        ImportDataFromDBM5();
                        break;
                }
            }
            else
            {
                oApplication.StatusBar.SetText("Please select valid Machine Type", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return;
            }
        }

        private void DeleteRecordFromTempTable()
        {
            try
            {
                var TempRecords = dbHrPayroll.TrnsTempAttendance.ToList();
                if (TempRecords != null && TempRecords.Count > 0)
                {
                    foreach (var detail in TempRecords)
                    {
                        dbHrPayroll.TrnsTempAttendance.DeleteOnSubmit(detail);
                    }
                    dbHrPayroll.SubmitChanges();
                    oApplication.StatusBar.SetText("Temprary attendance records refresh successfully", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                }
                else
                {
                    oApplication.StatusBar.SetText("No Record(s) Found", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("SaveRecords Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillValidEmployeeList()
        {
            try
            {
                var oEmpList = (from a in dbHrPayroll.MstEmployee where a.FlgActive == true select a).ToList();
                if (oEmpList.Count > 0)
                {
                    string valuelist = "";
                    for (int i = 0; i < oEmpList.Count; i++)
                    {
                        if ((i + 1) == oEmpList.Count)
                        {
                            valuelist += "'" + oEmpList[i].EmpID + "'";
                        }
                        else
                        {
                            valuelist += "'" + oEmpList[i].EmpID + "',";
                        }
                    }
                    ValidEmployeeList = valuelist;
                }
            }
            catch (Exception ex)
            {
            }
        }

        #endregion
    }
}
