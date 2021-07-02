using System;
using System.Data;
using System.Linq;
using DIHRMS;
using System.Globalization;
using System.Collections.Generic;
using System.Collections;

namespace ACHR.Screen
{
    class frm_fromSrch : HRMSBaseForm
    {
        #region "Global Variable Area"
        SAPbouiCOM.Button btnSearch;
        SAPbouiCOM.EditText txtSrch;
        SAPbouiCOM.DataTable dtEmpDetail;
        SAPbouiCOM.ComboBox cmb_Col;
        SAPbouiCOM.Matrix grd_Emp;
        SAPbouiCOM.Columns oColumns;
        SAPbouiCOM.Column oColumn, clNo, ID, FirstName, MiddleName, LastName, Department;

        #endregion

        #region "B1 Events"

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            try
            {
                oForm.Freeze(true);
                InitiallizeForm();
                FillColumns();
                FillEmployeeDeatilGrid();
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
                    Program.FromEmpId = id;                    
                    this.Dispose();
                    this.oForm.Close();
                }
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
                oForm.DataSources.UserDataSources.Add("txtSrch", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtSrch = oForm.Items.Item("txtSrch").Specific;
                txtSrch.DataBind.SetBound(true, "", "txtSrch");

                cmb_Col = oForm.Items.Item("cmb_Col").Specific;

                InitiallizegridMatrix();
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
                dtEmpDetail.Columns.Add("Department", SAPbouiCOM.BoFieldsType.ft_Text);

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

                oColumn = oColumns.Item("dptNm");
                Department = oColumn;
                oColumn.DataBind.Bind("EmpDetails", "Department");
                oColumn.TitleObject.Sortable = false;


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
            Int16 i = 0;
            try
            {
                string strOut = string.Empty;
                var Employees = dbHrPayroll.MstEmployee.Where(e => e.FlgActive == true && e.ResignDate == null).ToList();
                if (Convert.ToBoolean(Program.systemInfo.FlgEmployeeFilter))
                {
                    string strSql2 = "SELECT \"U_PayrollType\" FROM \"OUSR\" WHERE \"USER_CODE\" = '" + oCompany.UserName + "'";
                    SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                    //oRecSet.DoQuery("SELECT U_PayrollType FROM dbo.OUSR WHERE USER_CODE = '" + oCompany.UserName + "'");
                    oRecSet.DoQuery(strSql2);
                    strOut = Convert.ToString(oRecSet.Fields.Item("U_PayrollType").Value);

                    if (strOut != null && strOut != "")
                    {
                        Employees = Employees.Where(f => f.PayrollID.ToString() == strOut.Trim() && f.FlgActive == true && f.ResignDate != null).ToList();
                        //Employees = dbHrPayroll.MstEmployee.Where(e => e.FlgActive == true && e.PayrollID.ToString() == strOut.Trim()).ToList();

                        if (Employees != null && Employees.Count > 0)
                        {
                            //Employees = Employees.OrderBy(c => c.EmpID).ToList();
                            Employees = Employees.OrderBy(c => c.SortOrder).ToList();
                            dtEmpDetail.Rows.Clear();
                            dtEmpDetail.Rows.Add(Employees.Count());
                            foreach (var Emp in Employees)
                            {
                                dtEmpDetail.SetValue("No", i, i + 1);
                                dtEmpDetail.SetValue("ID", i, Emp.EmpID);
                                dtEmpDetail.SetValue("FirstName", i, string.IsNullOrEmpty(Emp.FirstName) ? "" : Emp.FirstName);
                                dtEmpDetail.SetValue("MiddleName", i, string.IsNullOrEmpty(Emp.MiddleName) ? "" : Emp.MiddleName);
                                dtEmpDetail.SetValue("LastName", i, string.IsNullOrEmpty(Emp.LastName) ? "" : Emp.LastName);
                                dtEmpDetail.SetValue("Department", i, string.IsNullOrEmpty(Emp.DepartmentName) ? "" : Emp.DepartmentName);
                                i++;
                            }
                            grd_Emp.LoadFromDataSource();
                        }
                    }
                    else
                    {


                        if (Employees != null && Employees.Count > 0)
                        {
                            //Employees = Employees.OrderBy(c => c.EmpID).ToList();
                            Employees = Employees.OrderBy(c => c.SortOrder).ToList();
                            dtEmpDetail.Rows.Clear();
                            dtEmpDetail.Rows.Add(Employees.Count());
                            foreach (var Emp in Employees)
                            {
                                dtEmpDetail.SetValue("No", i, i + 1);
                                dtEmpDetail.SetValue("ID", i, Emp.EmpID);
                                dtEmpDetail.SetValue("FirstName", i, string.IsNullOrEmpty(Emp.FirstName) ? "" : Emp.FirstName);
                                dtEmpDetail.SetValue("MiddleName", i, string.IsNullOrEmpty(Emp.MiddleName) ? "" : Emp.MiddleName);
                                dtEmpDetail.SetValue("LastName", i, string.IsNullOrEmpty(Emp.LastName) ? "" : Emp.LastName);
                                dtEmpDetail.SetValue("Department", i, string.IsNullOrEmpty(Emp.DepartmentName) ? "" : Emp.DepartmentName);
                                i++;
                            }
                            grd_Emp.LoadFromDataSource();
                        }
                    }
                }
                else
                {
                    if (Employees != null && Employees.Count > 0)
                    {
                        //Employees = Employees.OrderBy(c => c.EmpID).ToList();
                        Employees = Employees.OrderBy(c => c.SortOrder).ToList();
                        dtEmpDetail.Rows.Clear();
                        dtEmpDetail.Rows.Add(Employees.Count());
                        foreach (var Emp in Employees)
                        {
                            dtEmpDetail.SetValue("No", i, i + 1);
                            dtEmpDetail.SetValue("ID", i, Emp.EmpID);
                            dtEmpDetail.SetValue("FirstName", i, string.IsNullOrEmpty(Emp.FirstName) ? "" : Emp.FirstName);
                            dtEmpDetail.SetValue("MiddleName", i, string.IsNullOrEmpty(Emp.MiddleName) ? "" : Emp.MiddleName);
                            dtEmpDetail.SetValue("LastName", i, string.IsNullOrEmpty(Emp.LastName) ? "" : Emp.LastName);
                            dtEmpDetail.SetValue("Department", i, string.IsNullOrEmpty(Emp.DepartmentName) ? "" : Emp.DepartmentName);
                            i++;
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


                string strColumnName = cmb_Col.Value;
                string strValue = txtSrch.Value.ToLower();
                Int16 i = 0;
                var Employees = dbHrPayroll.MstEmployee.Where(e => e.FlgActive == true).ToList();

                switch (strColumnName)
                {
                    case "EmpID":
                        Employees = Employees.Where(f => f.EmpID.ToLower() == strValue).ToList();
                        break;
                    case "FirstName":
                        //Employees = Employees.Where(f => f.FirstName.ToLower().Contains(strValue)).ToList();
                        Employees = Employees.Where(f => f.FirstName.ToLower() == strValue).ToList();
                        break;
                    case "MiddleName":
                        //Employees = Employees.Where(f => f.MiddleName.ToLower().Contains(strValue)).ToList();
                        Employees = Employees.Where(f => f.MiddleName.ToLower() == strValue).ToList();
                        break;
                    case "LastName":
                        Employees = Employees.Where(f => f.LastName.ToLower() == strValue).ToList();
                        break;
                    default:
                        break;
                }
                dtEmpDetail.Rows.Clear();
                grd_Emp.LoadFromDataSource();
                if (Employees != null && Employees.Count > 0)
                {
                    dtEmpDetail.Rows.Clear();
                    dtEmpDetail.Rows.Add(Employees.Count());
                    foreach (var Emp in Employees)
                    {
                        dtEmpDetail.SetValue("No", i, i + 1);
                        dtEmpDetail.SetValue("ID", i, Emp.EmpID);
                        dtEmpDetail.SetValue("FirstName", i, string.IsNullOrEmpty(Emp.FirstName) ? "" : Emp.FirstName);
                        dtEmpDetail.SetValue("MiddleName", i, string.IsNullOrEmpty(Emp.MiddleName) ? "" : Emp.MiddleName);
                        dtEmpDetail.SetValue("LastName", i, string.IsNullOrEmpty(Emp.LastName) ? "" : Emp.LastName);
                        dtEmpDetail.SetValue("Department", i, string.IsNullOrEmpty(Emp.DepartmentName) ? "" : Emp.DepartmentName);
                        i++;    
                    }
                    grd_Emp.LoadFromDataSource();
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
