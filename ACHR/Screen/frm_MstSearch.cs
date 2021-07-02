
using System;
using System.Data;
using System.Linq;
using DIHRMS;
using System.Globalization;
using System.Collections.Generic;
using System.Collections;

namespace ACHR.Screen
{
    class frm_MstSearch : HRMSBaseForm
    {
        #region "Global Variable Area"
        //public SAPbouiCOM.Form oForm;
        //public SAPbobsCOM.Company oCompany;
        //public SAPbouiCOM.Application oApplication;

        SAPbouiCOM.Button btnSearch;
        
        SAPbouiCOM.EditText txtSrch;
        SAPbouiCOM.DataTable dtEmpDetail;
        SAPbouiCOM.ComboBox cmb_Col;
        SAPbouiCOM.Matrix grd_Emp;
        SAPbouiCOM.Columns oColumns;
        SAPbouiCOM.Column oColumn, clNo, ID, FirstName, MiddleName, LastName, Department, cl_ctp, cl_parl, cl_Des, cl_Pos, cl_Loc,cl_pass;
        IEnumerable<MstEmployee> oEmployees = null;
        int currentRowIndex = 1;
        string SearchText = "";
        #endregion

        #region "B1 Events"

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            try
            {
                oForm.Freeze(true);
                InitiallizeForm();
                oForm.ActiveItem = "txtSrch";       
                oForm.Freeze(false);

            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_MstShift Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            try
            {
                switch (pVal.ItemUID)
                {
                    case "btSrch":
                        FilterRecord();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_MstShift Function: etAfterClick Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        public override void etAfterDoubleClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterDoubleClick(ref pVal, ref BubbleEvent);
            if (pVal.ItemUID == "grd_Emp")
            {
                if (pVal.Row >= 1 && pVal.Row <= grd_Emp.RowCount)
                {
                    string id = Convert.ToString(dtEmpDetail.GetValue("ID", pVal.Row - 1));
                    Program.EmpID = id;
                    this.Dispose();
                    this.oForm.Close();
                }
            }

        }
        public override void etAfterKeyDown(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {

            base.etAfterKeyDown(ref pVal, ref BubbleEvent);
            try
            {
                if (pVal.ItemUID == "grd_Emp" || pVal.ItemUID == "txtSrch" || pVal.ItemUID == "cmb_Col")
                //if (pVal.ItemUID == "grd_Emp" || pVal.ItemUID == "cmb_Col")
                {
                    if (pVal.CharPressed == 13)
                    {

                        if (pVal.Row >= -1 && currentRowIndex >= 1)
                        {
                            if (currentRowIndex > 0)
                            {
                                if (SearchText == txtSrch.Value.Trim())
                                {
                                    string id = Convert.ToString(dtEmpDetail.GetValue("ID", currentRowIndex - 1));
                                    Program.EmpID = id;
                                    this.Dispose();
                                    this.oForm.Close();
                                }
                            }
                            else
                            {
                                grd_Emp.SelectRow(1, true, false);
                                SearchText = txtSrch.Value.Trim();
                            }
                        }
                    }
                    if (pVal.CharPressed == 38) //Arrow Up
                    {
                        if (pVal.Row != 0)
                        {
                            if (pVal.Row == -1 && currentRowIndex >= 0)
                            {
                                if (currentRowIndex > 1)
                                {
                                    currentRowIndex--;
                                }
                                grd_Emp.SelectRow(currentRowIndex, true, false);
                                SearchText = txtSrch.Value.Trim();
                                if (currentRowIndex <= 0)
                                {
                                    currentRowIndex = 1;
                                }
                            }
                        }
                    }
                    if (pVal.CharPressed == 40 && pVal.EventType == SAPbouiCOM.BoEventTypes.et_KEY_DOWN && pVal.BeforeAction == false) //Arrow Down
                    {
                        if (pVal.Row != 0)
                        {
                            if (pVal.Row == -1 && currentRowIndex > 0)
                            {
                                if (currentRowIndex < grd_Emp.RowCount)
                                {
                                    currentRowIndex++;
                                }
                                grd_Emp.SelectRow(currentRowIndex, true, false);
                                SearchText = txtSrch.Value.Trim();
                                if (currentRowIndex > grd_Emp.RowCount)
                                {
                                    currentRowIndex = 1;
                                }
                            }
                        }
                    }



                }

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("SetIDValue Error : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        #endregion

        #region "Local Methods"

        public void InitiallizeForm()
        {
            try
            {
                btnSearch = oForm.Items.Item("btSrch").Specific;
                //Initializing Textboxes
                //oForm.DataSources.UserDataSources.Add("txtSrch", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtSrch = oForm.Items.Item("txtSrch").Specific;
                //txtSrch.DataBind.SetBound(true, "", "txtSrch");
                cmb_Col = oForm.Items.Item("cmb_Col").Specific;
                FillColumns();
                InitiallizegridMatrix();
                FillEmployeeDeatilGrid();
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
                dtEmpDetail = oForm.DataSources.DataTables.Add("EmpDetails");
                dtEmpDetail.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtEmpDetail.Columns.Add("ID", SAPbouiCOM.BoFieldsType.ft_Text);
                dtEmpDetail.Columns.Add("FirstName", SAPbouiCOM.BoFieldsType.ft_Text);
                dtEmpDetail.Columns.Add("MiddleName", SAPbouiCOM.BoFieldsType.ft_Text);
                dtEmpDetail.Columns.Add("LastName", SAPbouiCOM.BoFieldsType.ft_Text);
                dtEmpDetail.Columns.Add("ContractType", SAPbouiCOM.BoFieldsType.ft_Text);
                dtEmpDetail.Columns.Add("Payroll", SAPbouiCOM.BoFieldsType.ft_Text);
                dtEmpDetail.Columns.Add("Designation", SAPbouiCOM.BoFieldsType.ft_Text);
                dtEmpDetail.Columns.Add("Position", SAPbouiCOM.BoFieldsType.ft_Text);
                dtEmpDetail.Columns.Add("Location", SAPbouiCOM.BoFieldsType.ft_Text);             
                dtEmpDetail.Columns.Add("Department", SAPbouiCOM.BoFieldsType.ft_Text);
                dtEmpDetail.Columns.Add("IqamaNo", SAPbouiCOM.BoFieldsType.ft_Text);
                dtEmpDetail.Columns.Add("PassportNo", SAPbouiCOM.BoFieldsType.ft_Text);

                grd_Emp = (SAPbouiCOM.Matrix)oForm.Items.Item("grd_Emp").Specific;
                oColumns = (SAPbouiCOM.Columns)grd_Emp.Columns;


                oColumn = oColumns.Item("No");
                clNo = oColumn;
                oColumn.DataBind.Bind("EmpDetails", "No");

                oColumn = oColumns.Item("cl_ID");
                ID = oColumn;
                oColumn.DataBind.Bind("EmpDetails", "ID");
                oColumn.TitleObject.Sortable = false;

                oColumn = oColumns.Item("cl_FName");
                FirstName = oColumn;
                oColumn.DataBind.Bind("EmpDetails", "FirstName");
                oColumn.TitleObject.Sortable = false;

                oColumn = oColumns.Item("cl_MName");
                MiddleName = oColumn;
                oColumn.DataBind.Bind("EmpDetails", "MiddleName");
                oColumn.TitleObject.Sortable = false;

                oColumn = oColumns.Item("cl_LName");
                LastName = oColumn;
                oColumn.DataBind.Bind("EmpDetails", "LastName");
                oColumn.TitleObject.Sortable = false;

                oColumn = oColumns.Item("cl_ctp");
                cl_ctp = oColumn;
                oColumn.DataBind.Bind("EmpDetails", "ContractType");
                oColumn.TitleObject.Sortable = false;

                oColumn = oColumns.Item("cl_parl");
                cl_parl = oColumn;
                oColumn.DataBind.Bind("EmpDetails", "Payroll");
                oColumn.TitleObject.Sortable = false;

                oColumn = oColumns.Item("cl_Des");
                cl_Des = oColumn;
                oColumn.DataBind.Bind("EmpDetails", "Designation");
                oColumn.TitleObject.Sortable = false;

                oColumn = oColumns.Item("cl_Pos");
                cl_Pos = oColumn;
                oColumn.DataBind.Bind("EmpDetails", "Position");
                oColumn.TitleObject.Sortable = false;

                oColumn = oColumns.Item("cl_Loc");
                cl_Loc = oColumn;
                oColumn.DataBind.Bind("EmpDetails", "Location");
                oColumn.TitleObject.Sortable = false;

                oColumn = oColumns.Item("dptNm");
                Department = oColumn;
                oColumn.DataBind.Bind("EmpDetails", "Department");
                oColumn.TitleObject.Sortable = false;

                oColumn = oColumns.Item("V_0");
                Department = oColumn;
                oColumn.DataBind.Bind("EmpDetails", "IqamaNo");
                oColumn.TitleObject.Sortable = false;

                oColumn = oColumns.Item("V_1");
                cl_pass = oColumn;
                oColumn.DataBind.Bind("EmpDetails", "PassportNo");
                oColumn.TitleObject.Sortable = false;

                grd_Emp.SelectionMode = SAPbouiCOM.BoMatrixSelect.ms_Auto;
                


            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
         
        private void FillColumns()
        {
            try
            {
                int i = 0;
                ArrayList list = new ArrayList();
                list.Add("EmpID");
                list.Add("FirstName");
                list.Add("MiddleName");
                list.Add("LastName");
                list.Add("DepartmentName");
                list.Add("LocationName");
                list.Add("PositionName");
                list.Add("DesignationName");
                list.Add("PayrollName");
                list.Add("ContractType");
                list.Add("IqamaNo");
                list.Add("PassportNo");

                if (list.Count > 0)
                {
                    cmb_Col.ValidValues.Add("-1", "[select one]");
                    foreach (var v in list)
                    {
                        cmb_Col.ValidValues.Add(Convert.ToString(v), Convert.ToString(v));
                    }
                    cmb_Col.Select(1, SAPbouiCOM.BoSearchKey.psk_Index);
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("FillColumns Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void FillEmployeeDeatilGrid()
        {
            try
            {
                DataTable dt = new DataTable();
                string strOut = string.Empty, strSql = string.Empty;
                strSql = sqlString.getSql(Program.sqlString, SearchKeyVal);
                if (Convert.ToBoolean(Program.systemInfo.FlgEmployeeFilter.GetValueOrDefault()))
                {
                    if (!string.IsNullOrEmpty(Program.objHrmsUI.EmployeeFilterValues))
                    {
                        strSql = strSql + " And A1.PayrollID In (" + Program.objHrmsUI.EmployeeFilterValues + ")";
                        strSql += " ORDER BY A1.SortOrder Asc ";
                        dt = ds.getDataTable(strSql);
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            dtEmpDetail.Rows.Clear();
                            dtEmpDetail.Rows.Add(dt.Rows.Count);
                            for (int K = 0; K < dt.Rows.Count; K++)
                            {
                                string ID = dt.Rows[K]["ID"].ToString();
                                string EmpID = dt.Rows[K]["EmpID"].ToString();
                                string FirstName = dt.Rows[K]["FirstName"].ToString();
                                string MiddleName = dt.Rows[K]["MiddleName"].ToString();
                                string LastName = dt.Rows[K]["LastName"].ToString();
                                string Depart = dt.Rows[K]["DepartmentName"].ToString();
                                string contractType = dt.Rows[K]["EmployeeContractType"].ToString();
                                string payrollName = dt.Rows[K]["PayrollName"].ToString();
                                string DesignationName = dt.Rows[K]["DesignationName"].ToString();
                                string positionName = dt.Rows[K]["PositionName"].ToString();
                                string locationName = dt.Rows[K]["LocationName"].ToString();
                                string IqamaNo = dt.Rows[K]["IDNo"].ToString();
                                string PassPortNo = dt.Rows[K]["PassportNo"].ToString();

                                dtEmpDetail.SetValue("No", K, K + 1);
                                dtEmpDetail.SetValue("ID", K, EmpID);
                                dtEmpDetail.SetValue("FirstName", K, string.IsNullOrEmpty(FirstName) ? "" : FirstName);
                                dtEmpDetail.SetValue("MiddleName", K, string.IsNullOrEmpty(MiddleName) ? "" : MiddleName);
                                dtEmpDetail.SetValue("LastName", K, string.IsNullOrEmpty(LastName) ? "" : LastName);
                                dtEmpDetail.SetValue("Department", K, string.IsNullOrEmpty(Depart) ? "" : Depart);
                                dtEmpDetail.SetValue("ContractType", K, string.IsNullOrEmpty(contractType) ? "" : contractType);
                                dtEmpDetail.SetValue("Payroll", K, string.IsNullOrEmpty(payrollName) ? "" : payrollName);
                                dtEmpDetail.SetValue("Designation", K, string.IsNullOrEmpty(DesignationName) ? "" : DesignationName);
                                dtEmpDetail.SetValue("Position", K, string.IsNullOrEmpty(positionName) ? "" : positionName);
                                dtEmpDetail.SetValue("Location", K, string.IsNullOrEmpty(locationName) ? "" : locationName);
                                dtEmpDetail.SetValue("IqamaNo", K, string.IsNullOrEmpty(IqamaNo) ? "" : IqamaNo);
                                dtEmpDetail.SetValue("PassportNo", K, string.IsNullOrEmpty(PassPortNo) ? "" : PassPortNo);
                            }
                            grd_Emp.LoadFromDataSource();
                        }
                    }                    
                    else
                    {
                        strSql += " ORDER BY A1.SortOrder Asc ";
                        dt = ds.getDataTable(strSql);
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            dtEmpDetail.Rows.Clear();
                            dtEmpDetail.Rows.Add(dt.Rows.Count);
                            for (int K = 0; K < dt.Rows.Count; K++)
                            {
                                string ID = dt.Rows[K]["ID"].ToString();
                                string EmpID = dt.Rows[K]["EmpID"].ToString();
                                string FirstName = dt.Rows[K]["FirstName"].ToString();
                                string MiddleName = dt.Rows[K]["MiddleName"].ToString();
                                string LastName = dt.Rows[K]["LastName"].ToString();
                                string Depart = dt.Rows[K]["DepartmentName"].ToString();
                                string contractType = dt.Rows[K]["EmployeeContractType"].ToString();
                                string payrollName = dt.Rows[K]["PayrollName"].ToString();
                                string DesignationName = dt.Rows[K]["DesignationName"].ToString();
                                string positionName = dt.Rows[K]["PositionName"].ToString();
                                string locationName = dt.Rows[K]["LocationName"].ToString();
                                string IqamaNo = dt.Rows[K]["IDNo"].ToString();
                                string PassPortNo = dt.Rows[K]["PassportNo"].ToString();

                                dtEmpDetail.SetValue("No", K, K + 1);
                                dtEmpDetail.SetValue("ID", K, EmpID);
                                dtEmpDetail.SetValue("FirstName", K, string.IsNullOrEmpty(FirstName) ? "" : FirstName);
                                dtEmpDetail.SetValue("MiddleName", K, string.IsNullOrEmpty(MiddleName) ? "" : MiddleName);
                                dtEmpDetail.SetValue("LastName", K, string.IsNullOrEmpty(LastName) ? "" : LastName);
                                dtEmpDetail.SetValue("Department", K, string.IsNullOrEmpty(Depart) ? "" : Depart);
                                dtEmpDetail.SetValue("ContractType", K, string.IsNullOrEmpty(contractType) ? "" : contractType);
                                dtEmpDetail.SetValue("Payroll", K, string.IsNullOrEmpty(payrollName) ? "" : payrollName);
                                dtEmpDetail.SetValue("Designation", K, string.IsNullOrEmpty(DesignationName) ? "" : DesignationName);
                                dtEmpDetail.SetValue("Position", K, string.IsNullOrEmpty(positionName) ? "" : positionName);
                                dtEmpDetail.SetValue("Location", K, string.IsNullOrEmpty(locationName) ? "" : locationName);
                                dtEmpDetail.SetValue("IqamaNo", K, string.IsNullOrEmpty(IqamaNo) ? "" : IqamaNo);
                                dtEmpDetail.SetValue("PassportNo", K, string.IsNullOrEmpty(PassPortNo) ? "" : PassPortNo);
                            }
                            grd_Emp.LoadFromDataSource();
                        }
                    }
                }
                else
                {
                    strSql += " ORDER BY A1.SortOrder Asc ";
                    dt = ds.getDataTable(strSql);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        dtEmpDetail.Rows.Clear();
                        dtEmpDetail.Rows.Add(dt.Rows.Count);
                        for (int K = 0; K < dt.Rows.Count; K++)
                        {
                            string ID = dt.Rows[K]["ID"].ToString();
                            string EmpID = dt.Rows[K]["EmpID"].ToString();
                            string FirstName = dt.Rows[K]["FirstName"].ToString();
                            string MiddleName = dt.Rows[K]["MiddleName"].ToString();
                            string LastName = dt.Rows[K]["LastName"].ToString();
                            string Depart = dt.Rows[K]["DepartmentName"].ToString();
                            string contractType = dt.Rows[K]["EmployeeContractType"].ToString();
                            string payrollName = dt.Rows[K]["PayrollName"].ToString();
                            string DesignationName = dt.Rows[K]["DesignationName"].ToString();
                            string positionName = dt.Rows[K]["PositionName"].ToString();
                            string locationName = dt.Rows[K]["LocationName"].ToString();
                            string IqamaNo = dt.Rows[K]["IDNo"].ToString();
                            string PassPortNo = dt.Rows[K]["PassportNo"].ToString();

                            dtEmpDetail.SetValue("No", K, K + 1);
                            dtEmpDetail.SetValue("ID", K, EmpID);
                            dtEmpDetail.SetValue("FirstName", K, string.IsNullOrEmpty(FirstName) ? "" : FirstName);
                            dtEmpDetail.SetValue("MiddleName", K, string.IsNullOrEmpty(MiddleName) ? "" : MiddleName);
                            dtEmpDetail.SetValue("LastName", K, string.IsNullOrEmpty(LastName) ? "" : LastName);
                            dtEmpDetail.SetValue("Department", K, string.IsNullOrEmpty(Depart) ? "" : Depart);
                            dtEmpDetail.SetValue("ContractType", K, string.IsNullOrEmpty(contractType) ? "" : contractType);
                            dtEmpDetail.SetValue("Payroll", K, string.IsNullOrEmpty(payrollName) ? "" : payrollName);
                            dtEmpDetail.SetValue("Designation", K, string.IsNullOrEmpty(DesignationName) ? "" : DesignationName);
                            dtEmpDetail.SetValue("Position", K, string.IsNullOrEmpty(positionName) ? "" : positionName);
                            dtEmpDetail.SetValue("Location", K, string.IsNullOrEmpty(locationName) ? "" : locationName);
                            dtEmpDetail.SetValue("IqamaNo", K, string.IsNullOrEmpty(IqamaNo) ? "" : IqamaNo);
                            dtEmpDetail.SetValue("PassportNo", K, string.IsNullOrEmpty(PassPortNo) ? "" : PassPortNo);
                        }
                        grd_Emp.LoadFromDataSource();
                    }
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("FillEmployeeDeatilGrid Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void FilterRecord()
        {
            try
            {
                DataTable dt = new DataTable();
                string strColumnName = cmb_Col.Value;
                string strOut = string.Empty;
                string strValue = txtSrch.Value.ToLower();
                string strSql = sqlString.getSql(Program.sqlString, SearchKeyVal);
                if (Convert.ToBoolean(Program.systemInfo.FlgEmployeeFilter))
                {
                    string strSql2 = "SELECT \"U_PayrollType\" FROM \"OUSR\" WHERE \"USER_CODE\" = '" + oCompany.UserName + "'";
                    SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                    //oRecSet.DoQuery("SELECT U_PayrollType FROM dbo.OUSR WHERE USER_CODE = '" + oCompany.UserName + "'");
                    
                    oRecSet.DoQuery(strSql2);
                    strOut = Convert.ToString(oRecSet.Fields.Item("U_PayrollType").Value);
                    //strOut = oRecSet.Fields.Item("U_PayrollType").Value;
                    strSql = strSql + " And A1.PayrollID in (" + strOut + ")";
                    strSql += " ORDER BY A1.SortOrder Asc ";
                    dt = ds.getDataTable(strSql);

                    DataView dv = dt.DefaultView;

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(strValue))
                        {
                            switch (strColumnName)
                            {
                                case "EmpID":
                                    dv.RowFilter = "EmpID ='" + strValue + "'";
                                    //Employees = Employees.Where(f => f.EmpID == strValue).ToList();
                                    break;
                                case "FirstName":
                                    dv.RowFilter = "FirstName ='" + strValue + "'";
                                    //Employees = Employees.Where(f => f.FirstName.ToLower() == strValue).ToList();
                                    break;
                                case "MiddleName":
                                    dv.RowFilter = "MiddleName ='" + strValue + "'";
                                    //Employees = Employees.Where(f => f.MiddleName.ToLower().Contains(strValue)).ToList();
                                    //Employees = Employees.Where(f => f.MiddleName.ToLower() == strValue).ToList();
                                    break;
                                case "LastName":
                                    dv.RowFilter = "LastName ='" + strValue + "'";
                                    //Employees = Employees.Where(f => f.LastName.ToLower() == strValue).ToList();
                                    break;
                                case "DepartmentName":
                                    dv.RowFilter = "DepartmentName ='" + strValue + "'";
                                    //Employees = Employees.Where(f => f.LastName.ToLower() == strValue).ToList();
                                    break;
                                case "LocationName":
                                    dv.RowFilter = "LocationName ='" + strValue + "'";
                                    //Employees = Employees.Where(f => f.LastName.ToLower() == strValue).ToList();
                                    break;
                                case "PositionName":
                                    dv.RowFilter = "PositionName ='" + strValue + "'";
                                    //Employees = Employees.Where(f => f.LastName.ToLower() == strValue).ToList();
                                    break;
                                case "DesignationName":
                                    dv.RowFilter = "DesignationName ='" + strValue + "'";
                                    //Employees = Employees.Where(f => f.LastName.ToLower() == strValue).ToList();
                                    break;
                                case "PayrollName":
                                    dv.RowFilter = "PayrollName ='" + strValue + "'";
                                    //Employees = Employees.Where(f => f.LastName.ToLower() == strValue).ToList();
                                    break;
                                case "ContractType":
                                    dv.RowFilter = "EmployeeContractType ='" + strValue + "'";
                                    //Employees = Employees.Where(f => f.LastName.ToLower() == strValue).ToList();
                                    break;
                                case "IqamaNo":
                                    dv.RowFilter = "IDNo ='" + strValue + "'";
                                    break;
                                case "PassportNo":
                                    dv.RowFilter = "PassportNo ='" + strValue + "'";
                                    break;
                                default:
                                    break;
                            }
                        }
                        dt = dv.ToTable();
                        dtEmpDetail.Rows.Clear();
                        dtEmpDetail.Rows.Add(dt.Rows.Count);
                        for (int K = 0; K < dt.Rows.Count; K++)
                        {
                            string ID = dt.Rows[K]["ID"].ToString();
                            string EmpID = dt.Rows[K]["EmpID"].ToString();
                            string FirstName = dt.Rows[K]["FirstName"].ToString();
                            string MiddleName = dt.Rows[K]["MiddleName"].ToString();
                            string LastName = dt.Rows[K]["LastName"].ToString();
                            string Depart = dt.Rows[K]["DepartmentName"].ToString();
                            string contractType = dt.Rows[K]["EmployeeContractType"].ToString();
                            string payrollName = dt.Rows[K]["PayrollName"].ToString();
                            string DesignationName = dt.Rows[K]["DesignationName"].ToString();
                            string positionName = dt.Rows[K]["PositionName"].ToString();
                            string locationName = dt.Rows[K]["LocationName"].ToString();
                            string IqamaNo = dt.Rows[K]["IDNo"].ToString();
                            string PassPortNo = dt.Rows[K]["PassportNo"].ToString();

                            dtEmpDetail.SetValue("No", K, K + 1);
                            dtEmpDetail.SetValue("ID", K, EmpID);
                            dtEmpDetail.SetValue("FirstName", K, string.IsNullOrEmpty(FirstName) ? "" : FirstName);
                            dtEmpDetail.SetValue("MiddleName", K, string.IsNullOrEmpty(MiddleName) ? "" : MiddleName);
                            dtEmpDetail.SetValue("LastName", K, string.IsNullOrEmpty(LastName) ? "" : LastName);
                            dtEmpDetail.SetValue("Department", K, string.IsNullOrEmpty(Depart) ? "" : Depart);
                            dtEmpDetail.SetValue("ContractType", K, string.IsNullOrEmpty(contractType) ? "" : contractType);
                            dtEmpDetail.SetValue("Payroll", K, string.IsNullOrEmpty(payrollName) ? "" : payrollName);
                            dtEmpDetail.SetValue("Designation", K, string.IsNullOrEmpty(DesignationName) ? "" : DesignationName);
                            dtEmpDetail.SetValue("Position", K, string.IsNullOrEmpty(positionName) ? "" : positionName);
                            dtEmpDetail.SetValue("Location", K, string.IsNullOrEmpty(locationName) ? "" : locationName);
                            dtEmpDetail.SetValue("IqamaNo", K, string.IsNullOrEmpty(IqamaNo) ? "" : IqamaNo);
                            dtEmpDetail.SetValue("PassportNo", K, string.IsNullOrEmpty(PassPortNo) ? "" : PassPortNo);
                        }
                        grd_Emp.LoadFromDataSource();

                    }
                }
                else
                {
                    strSql += " ORDER BY A1.SortOrder Asc ";
                    dt = ds.getDataTable(strSql);
                    DataView dv = dt.DefaultView;
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(strValue))
                        {
                            switch (strColumnName)
                            {
                                case "EmpID":
                                    dv.RowFilter = "EmpID ='" + strValue + "'";
                                    //Employees = Employees.Where(f => f.EmpID == strValue).ToList();
                                    break;
                                case "FirstName":
                                    dv.RowFilter = "FirstName ='" + strValue + "'";
                                    //Employees = Employees.Where(f => f.FirstName.ToLower() == strValue).ToList();
                                    break;
                                case "MiddleName":
                                    dv.RowFilter = "MiddleName ='" + strValue + "'";
                                    //Employees = Employees.Where(f => f.MiddleName.ToLower().Contains(strValue)).ToList();
                                    //Employees = Employees.Where(f => f.MiddleName.ToLower() == strValue).ToList();
                                    break;
                                case "LastName":
                                    dv.RowFilter = "LastName ='" + strValue + "'";
                                    //Employees = Employees.Where(f => f.LastName.ToLower() == strValue).ToList();
                                    break;
                                case "DepartmentName":
                                    dv.RowFilter = "DepartmentName ='" + strValue + "'";
                                    //Employees = Employees.Where(f => f.LastName.ToLower() == strValue).ToList();
                                    break;
                                case "LocationName":
                                    dv.RowFilter = "LocationName ='" + strValue + "'";
                                    //Employees = Employees.Where(f => f.LastName.ToLower() == strValue).ToList();
                                    break;
                                case "PositionName":
                                    dv.RowFilter = "PositionName ='" + strValue + "'";
                                    //Employees = Employees.Where(f => f.LastName.ToLower() == strValue).ToList();
                                    break;
                                case "DesignationName":
                                    dv.RowFilter = "DesignationName ='" + strValue + "'";
                                    //Employees = Employees.Where(f => f.LastName.ToLower() == strValue).ToList();
                                    break;
                                case "PayrollName":
                                    dv.RowFilter = "PayrollName ='" + strValue + "'";
                                    //Employees = Employees.Where(f => f.LastName.ToLower() == strValue).ToList();
                                    break;
                                case "ContractType":
                                    dv.RowFilter = "EmployeeContractType ='" + strValue + "'";
                                    //Employees = Employees.Where(f => f.LastName.ToLower() == strValue).ToList();
                                    break;
                                case "IqamaNo":
                                    dv.RowFilter = "IDNo ='" + strValue + "'";
                                    break;
                                case "PassportNo":
                                    dv.RowFilter = "PassportNo ='" + strValue + "'";
                                    break;
                                default:
                                    break;
                            }
                        }
                        dt = dv.ToTable();
                        dtEmpDetail.Rows.Clear();
                        dtEmpDetail.Rows.Add(dt.Rows.Count);
                        for (int K = 0; K < dt.Rows.Count; K++)
                        {
                            string ID = dt.Rows[K]["ID"].ToString();
                            string EmpID = dt.Rows[K]["EmpID"].ToString();
                            string FirstName = dt.Rows[K]["FirstName"].ToString();
                            string MiddleName = dt.Rows[K]["MiddleName"].ToString();
                            string LastName = dt.Rows[K]["LastName"].ToString();
                            string Depart = dt.Rows[K]["DepartmentName"].ToString();
                            string contractType = dt.Rows[K]["EmployeeContractType"].ToString();
                            string payrollName = dt.Rows[K]["PayrollName"].ToString();
                            string DesignationName = dt.Rows[K]["DesignationName"].ToString();
                            string positionName = dt.Rows[K]["PositionName"].ToString();
                            string locationName = dt.Rows[K]["LocationName"].ToString();
                            string IqamaNo = dt.Rows[K]["IDNo"].ToString();
                            string PassPortNo = dt.Rows[K]["PassportNo"].ToString();

                            dtEmpDetail.SetValue("No", K, K + 1);
                            dtEmpDetail.SetValue("ID", K, EmpID);
                            dtEmpDetail.SetValue("FirstName", K, string.IsNullOrEmpty(FirstName) ? "" : FirstName);
                            dtEmpDetail.SetValue("MiddleName", K, string.IsNullOrEmpty(MiddleName) ? "" : MiddleName);
                            dtEmpDetail.SetValue("LastName", K, string.IsNullOrEmpty(LastName) ? "" : LastName);
                            dtEmpDetail.SetValue("Department", K, string.IsNullOrEmpty(Depart) ? "" : Depart);
                            dtEmpDetail.SetValue("ContractType", K, string.IsNullOrEmpty(contractType) ? "" : contractType);
                            dtEmpDetail.SetValue("Payroll", K, string.IsNullOrEmpty(payrollName) ? "" : payrollName);
                            dtEmpDetail.SetValue("Designation", K, string.IsNullOrEmpty(DesignationName) ? "" : DesignationName);
                            dtEmpDetail.SetValue("Position", K, string.IsNullOrEmpty(positionName) ? "" : positionName);
                            dtEmpDetail.SetValue("Location", K, string.IsNullOrEmpty(locationName) ? "" : locationName);
                            dtEmpDetail.SetValue("IqamaNo", K, string.IsNullOrEmpty(IqamaNo) ? "" : IqamaNo);
                            dtEmpDetail.SetValue("PassportNo", K, string.IsNullOrEmpty(PassPortNo) ? "" : PassPortNo);
                        }
                        grd_Emp.LoadFromDataSource();
                    }

                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("FilterRecord Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void SetIDValue()
        {
            try
            {
                Program.EmpID = "";
                if (dtEmpDetail != null && dtEmpDetail.Rows.Count > 0)
                {
                    string strEmpID = (grd_Emp.Columns.Item("cl_ID").Cells.Item(1).Specific as SAPbouiCOM.EditText).Value;
                    Program.EmpID = strEmpID;
                    this.Dispose();
                    this.oForm.Close();
                }

            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("SetIDValue Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        #endregion
    }
}
