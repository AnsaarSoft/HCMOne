﻿using System;
using System.Data;
using System.Linq;
using DIHRMS;
using System.Globalization;
using System.Collections.Generic;
using System.Collections;

namespace ACHR.Screen
{
        class frm_Search : HRMSBaseForm
    {
        #region "Global Variable Area"
        SAPbouiCOM.Button btnSearch;
        SAPbouiCOM.EditText txtSrch;
        SAPbouiCOM.DataTable dtEmpDetail;
        SAPbouiCOM.ComboBox cmb_Col;
        SAPbouiCOM.Matrix grd_Emp;
        SAPbouiCOM.Columns oColumns;
        SAPbouiCOM.Column oColumn, clNo, ID, FirstName, MiddleName, LastName, Department,FullName;
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
            try
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
            catch (Exception ex)
            {
                
            }

        }
        public override void etAfterKeyDown(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {

            base.etAfterKeyDown(ref pVal, ref BubbleEvent);
            try
            {
                if (pVal.ItemUID == "grd_Emp" || pVal.ItemUID == "txtSrch" || pVal.ItemUID == "cmb_Col")
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
        public override void FindRecordMode()
        {
            base.FindRecordMode();
           // oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
            FillEmployeeDeatilGrid();
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
                dtEmpDetail.Columns.Add("FullName", SAPbouiCOM.BoFieldsType.ft_Text);
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

                oColumn = oColumns.Item("V_0");
                FullName = oColumn;
                oColumn.DataBind.Bind("EmpDetails", "FullName");
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
                list.Add("FullName");
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
                    cmb_Col.Select(2, SAPbouiCOM.BoSearchKey.psk_Index);
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
                if (string.IsNullOrEmpty(Program.EmpBasicSalary))
                { 
                    string strSql = sqlString.getSql(Program.sqlString, SearchKeyVal);
                    dt = ds.getDataTable(strSql);
                }
                if (!string.IsNullOrEmpty(Program.EmpBasicSalary) && Program.EmpBasicSalary != "0")
                {
                     
                    string strSqlLoan = sqlString.getSql(Program.sqlString, SearchKeyVal);
                    dt = ds.getDataTable(strSqlLoan);

                    DataView dv = new DataView(dt);
                    dv.RowFilter = string.Format("BasicSalary >= '" + Program.EmpBasicSalary + "'");
                   
                    DataTable dtNew = dv.ToTable();

                    dt = dtNew;
                }
                
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
                        string FullName = dt.Rows[K]["FullName"].ToString();
                        dtEmpDetail.SetValue("No", K, K + 1);
                        dtEmpDetail.SetValue("ID", K, EmpID);
                        dtEmpDetail.SetValue("FirstName", K, string.IsNullOrEmpty(FirstName) ? "" : FirstName);
                        dtEmpDetail.SetValue("MiddleName", K, string.IsNullOrEmpty(MiddleName) ? "" : MiddleName);
                        dtEmpDetail.SetValue("LastName", K, string.IsNullOrEmpty(LastName) ? "" : LastName);
                        
                        dtEmpDetail.SetValue("Department", K, string.IsNullOrEmpty(Depart) ? "" : Depart);
                        dtEmpDetail.SetValue("FullName", K, string.IsNullOrEmpty(FullName) ? "" : FullName); 
                    }
                    grd_Emp.LoadFromDataSource();

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
                string strValue = txtSrch.Value.ToLower();
                string strSql = sqlString.getSql(Program.sqlString, SearchKeyVal);                
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
                            case "FullName":
                                dv.RowFilter = "FullName LIKE '%" + strValue + "%'";
                                //Employees = Employees.Where(f => f.LastName.ToLower() == strValue).ToList();
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
                        string FullName = dt.Rows[K]["FullName"].ToString();
                        dtEmpDetail.SetValue("No", K, K + 1);
                        dtEmpDetail.SetValue("ID", K, EmpID);
                        dtEmpDetail.SetValue("FirstName", K, string.IsNullOrEmpty(FirstName) ? "" : FirstName);
                        dtEmpDetail.SetValue("MiddleName", K, string.IsNullOrEmpty(MiddleName) ? "" : MiddleName);
                        dtEmpDetail.SetValue("LastName", K, string.IsNullOrEmpty(LastName) ? "" : LastName);
                        dtEmpDetail.SetValue("Department", K, string.IsNullOrEmpty(Depart) ? "" : Depart);
                        dtEmpDetail.SetValue("FullName", K, string.IsNullOrEmpty(FullName) ? "" : FullName);
                    }
                    grd_Emp.LoadFromDataSource();

                }    

                //Int16 i = 0;

                //var Employees = dbHrPayroll.MstEmployee.Where(e => e.FlgActive == true).ToList();

                //switch (strColumnName)
                //{
                //    case "EmpID":
                //        Employees = Employees.Where(f => f.EmpID == strValue).ToList();
                //        break;
                //    case "FirstName":
                //        //Employees = Employees.Where(f => f.FirstName.ToLower().Contains(strValue)).ToList();
                //        Employees = Employees.Where(f => f.FirstName.ToLower() == strValue).ToList();
                //        break;
                //    case "MiddleName":
                //        //Employees = Employees.Where(f => f.MiddleName.ToLower().Contains(strValue)).ToList();
                //        Employees = Employees.Where(f => f.MiddleName.ToLower() == strValue).ToList();
                //        break;
                //    case "LastName":
                //        Employees = Employees.Where(f => f.LastName.ToLower() == strValue).ToList();
                //        break;
                //    default:
                //        break;
                //}
                //dtEmpDetail.Rows.Clear();
                //grd_Emp.LoadFromDataSource();
                //if (Employees != null && Employees.Count > 0)
                //{
                //    dtEmpDetail.Rows.Clear();
                //    dtEmpDetail.Rows.Add(Employees.Count());
                //    foreach (var Emp in Employees)
                //    {
                //        dtEmpDetail.SetValue("No", i, i + 1);
                //        dtEmpDetail.SetValue("ID", i, Emp.EmpID);
                //        dtEmpDetail.SetValue("FirstName", i, string.IsNullOrEmpty(Emp.FirstName) ? "" : Emp.FirstName);
                //        dtEmpDetail.SetValue("MiddleName", i, string.IsNullOrEmpty(Emp.MiddleName) ? "" : Emp.MiddleName);
                //        dtEmpDetail.SetValue("LastName", i, string.IsNullOrEmpty(Emp.LastName) ? "" : Emp.LastName);
                //        dtEmpDetail.SetValue("Department", i, string.IsNullOrEmpty(Emp.DepartmentName) ? "" : Emp.DepartmentName);
                //        i++;
                //    }
                //    grd_Emp.LoadFromDataSource();
                //}

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
                if(dtEmpDetail!=null && dtEmpDetail.Rows.Count >0)
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
