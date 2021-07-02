using DIHRMS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace ACHR.Screen
{
    class frm_EmpAtch : HRMSBaseForm
    {

        #region Variables

        SAPbouiCOM.EditText txtEmployeeCode, txtEmployeeName, txtDesignation, txtManager, txtDateOfJoining, txtSalary;
        SAPbouiCOM.Matrix oMatrix;
        SAPbouiCOM.Columns oColumns;
        SAPbouiCOM.Column clCheck, clEmpId, clFileName, clViewAttach, clPath;
        SAPbouiCOM.DataTable oDataTable;
        SAPbouiCOM.Button btnAdd, btnCancel, btnLink, btnBrowse, btnPlus;
        SAPbouiCOM.StaticText lblFile;
        public string ImagePath = System.Windows.Forms.Application.StartupPath + "\\CFL.bmp", FilePath, FileName;
        int count = 0;

        #endregion

        #region SAP B1 Events

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            try
            {
                oForm.Freeze(true);

                InitiallizeForm();
                MatrixInitialize();
                if (!string.IsNullOrEmpty(Program.EmpID))
                {
                    LoadSelectedData(Program.EmpID);
                }
                oForm.Freeze(false);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_EmpAtch Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            if (pVal.ItemUID == "btnLink")
            {
                OpenNewSearchForm();
            }
            else if (pVal.ItemUID == "btnBrws")
            {
                BrowseAttachment();
            }
            else if (pVal.ItemUID == "btnPlus")
            {
                AddRowToMatrix();
            }
            else if (pVal.ItemUID == "DGVData")
            {
                if (pVal.ColUID == "Col_Atch")
                {
                    int MatRow = pVal.Row - 1;
                    ViewAttachment(MatRow);
                }
            }
            else if (pVal.ItemUID == "btnAdd")
            {
                InsertAttachmentRequest();
            }
            else if (pVal.ItemUID == "btnCncl")
            {
                this.Dispose();
                this.oForm.Close();
            }
        }
        public override void etFormAfterActivate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etFormAfterActivate(ref pVal, ref BubbleEvent);
            if (txtEmployeeCode == null)
            {
                Program.EmpID = string.Empty;
                return;
            }
            if (Program.EmpID == string.Empty)
            {
                return;
            }
            if (Program.EmpID == txtEmployeeCode.Value)
            {
                return;
            }
            else
            {
                SetEmpValues();
            }
        }
        #endregion

        #region Functions

        public void InitiallizeForm()
        {
            try
            {
                txtEmployeeCode = oForm.Items.Item("txtEmpID").Specific;
                txtEmployeeName = oForm.Items.Item("txtEmpNm").Specific;
                txtDesignation = oForm.Items.Item("txtDesg").Specific;
                txtManager = oForm.Items.Item("txtMgr").Specific;
                txtDateOfJoining = oForm.Items.Item("txtDTJoin").Specific;
                oForm.DataSources.UserDataSources.Add("DateJoin", SAPbouiCOM.BoDataType.dt_DATE);
                txtDateOfJoining.DataBind.SetBound(true, "", "DateJoin");

                txtSalary = oForm.Items.Item("txtSlry").Specific;
                oForm.DataSources.UserDataSources.Add("Salary", SAPbouiCOM.BoDataType.dt_PRICE);
                txtSalary.DataBind.SetBound(true, "", "Salary");
                oMatrix = oForm.Items.Item("DGVData").Specific;
                oMatrix.AutoResizeColumns();
                oDataTable = oForm.DataSources.DataTables.Add("DGVEmp");

                btnAdd = oForm.Items.Item("btnAdd").Specific;
                btnCancel = oForm.Items.Item("btnCncl").Specific;
                btnLink = oForm.Items.Item("btnLink").Specific;
                btnBrowse = oForm.Items.Item("btnBrws").Specific;
                btnPlus = oForm.Items.Item("btnPlus").Specific;

                lblFile = oForm.Items.Item("lblFile").Specific;

                oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                InitiallizeDocument();
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Initialize Form Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        public void InitiallizeDocument()
        {
            try
            {
                if (!string.IsNullOrEmpty(Program.AttachEmpID))
                {
                    Program.EmpID = Program.AttachEmpID;
                    SetEmpValues();
                }
            }
            catch (Exception Ex)
            {
            }
        }
        public void MatrixInitialize()
        {
            try
            {
                oColumns = (SAPbouiCOM.Columns)oMatrix.Columns;

                oDataTable.Columns.Add("Check", SAPbouiCOM.BoFieldsType.ft_Text);
                oDataTable.Columns.Add("EmpId", SAPbouiCOM.BoFieldsType.ft_Integer);
                oDataTable.Columns.Add("FileName", SAPbouiCOM.BoFieldsType.ft_Text);
                oDataTable.Columns.Add("ViewAttch", SAPbouiCOM.BoFieldsType.ft_Text);
                oDataTable.Columns.Add("Path", SAPbouiCOM.BoFieldsType.ft_Text);
                oDataTable.Rows.Clear();

                clCheck = oColumns.Item("Col_Check");
                clCheck.DataBind.Bind("DGVEmp", "Check");

                clEmpId = oColumns.Item("Col_EmpId");
                clEmpId.DataBind.Bind("DGVEmp", "EmpId");

                clFileName = oColumns.Item("Col_FilNm");
                clFileName.DataBind.Bind("DGVEmp", "FileName");

                clViewAttach = oColumns.Item("Col_Atch");
                clViewAttach.DataBind.Bind("DGVEmp", "ViewAttch");

                clPath = oColumns.Item("Col_Path");
                clPath.DataBind.Bind("DGVEmp", "Path");
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Initialize Matrix Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        public void AddRowToMatrix()
        {
            oForm.Freeze(true);
            try
            {
                if (!string.IsNullOrWhiteSpace(FilePath))
                {
                    FileName = Path.GetFileName(FilePath);
                    oMatrix.FlushToDataSource();
                    oDataTable.Rows.Add(1);
                    oDataTable.SetValue("Check", count, "New");
                    oDataTable.SetValue("EmpId", count, txtEmployeeCode.Value);
                    oDataTable.SetValue("FileName", count, FileName);
                    oDataTable.SetValue("ViewAttch", count, FilePath);
                    oDataTable.SetValue("Path", count, FilePath);
                    oMatrix.LoadFromDataSource();
                    lblFile.Caption = "Select File: ";
                    FilePath = "";
                    FileName = "";
                    count++;
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Adding Row to Matrix Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            oForm.Freeze(false);
        }
        public void FillGrid()
        {
            oForm.Freeze(true);
            oDataTable.Rows.Clear();
            count = 0;
            try
            {
                oMatrix.FlushToDataSource();
                if (!string.IsNullOrWhiteSpace(txtEmployeeCode.Value))
                {
                    string pCode = txtEmployeeCode.Value;
                    var getEmp = (from a in dbHrPayroll.MstEmployee
                                  where a.EmpID.Contains(pCode)
                                  select a).FirstOrDefault();
                    if (getEmp != null)
                    {
                        var getAttachments = (from a in dbHrPayroll.MstEmployeeAttachment
                                              where a.EmpId == getEmp.ID
                                              select a).ToList();
                        foreach (var attch in getAttachments)
                        {
                            oDataTable.Rows.Add(1);
                            oDataTable.SetValue("Check", count, "Old");
                            oDataTable.SetValue("EmpId", count, attch.EmpId);
                            oDataTable.SetValue("FileName", count, attch.FileName);
                            oDataTable.SetValue("ViewAttch", count, attch.FilePath);
                            oDataTable.SetValue("Path", count, attch.FilePath);
                            count++;
                        }
                    }
                }
                oMatrix.LoadFromDataSource();
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Grid Failed to add record : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            oForm.Freeze(false);
        }
        private void OpenNewSearchForm()
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
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                }

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        public void BrowseAttachment()
        {
            try
            {
                FilePath = Program.objHrmsUI.FindFile();
                if (!string.IsNullOrEmpty(FilePath))
                {
                    lblFile.Caption = "Selected File: " + FilePath;
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("File Selection Error :" + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        public void ViewAttachment(int row)
        {
            oForm.Freeze(true);
            try
            {
                oMatrix.FlushToDataSource();
                for (int i = 0; i <= row; i++)
                {
                    string SelectAttachment = oDataTable.GetValue("ViewAttch", i);
                    if (SelectAttachment == "Y")
                    {
                        string ViewAttachment = oDataTable.GetValue("Path", i);
                        using (Process fileopener = new Process())
                        {
                            fileopener.StartInfo.FileName = ViewAttachment;
                            fileopener.StartInfo.Arguments = ViewAttachment;
                            fileopener.Start();
                        }
                    }
                }
                oMatrix.LoadFromDataSource();
                FillGrid();
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("View Attachment Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            oForm.Freeze(false);
        }
        private void LoadSelectedData(string pCode)
        {
            try
            {
                if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE)
                {
                    if (!string.IsNullOrEmpty(pCode))
                    {
                        var getEmp = (from a in dbHrPayroll.MstEmployee
                                      where a.EmpID.Contains(pCode)
                                      select a).FirstOrDefault();
                        if (getEmp != null)
                        {
                            txtEmployeeName.Value = getEmp.FirstName + " " + getEmp.MiddleName + " " + getEmp.LastName;
                            txtManager.Value = (from e in dbHrPayroll.MstEmployee where e.ID == getEmp.Manager select (e.FirstName + " " + e.MiddleName + " " + e.LastName)).FirstOrDefault();
                            txtDateOfJoining.Value = getEmp.JoiningDate == null ? "" : Convert.ToDateTime(getEmp.JoiningDate).ToString("yyyyMMdd");
                            txtDesignation.Value = getEmp.DesignationName;
                            txtSalary.Value = String.Format("{0:0.00}", ds.getEmpGross(getEmp));
                            FillGrid();
                        }

                    }
                }
                else
                {

                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_EmpAtch Function: LoadSelectedData Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void SetEmpValues()
        {
            try
            {
                if (!string.IsNullOrEmpty(Program.EmpID))
                {
                    txtEmployeeCode.Value = Program.EmpID;
                    LoadSelectedData(txtEmployeeCode.Value);
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }        
        private void InsertAttachmentRequest()
        {
            oForm.Freeze(true);
            oMatrix.FlushToDataSource();
            MstEmployeeAttachment objAttachment = null;
            try
            {
                string pCode = txtEmployeeCode.Value;
                var getEmp = (from a in dbHrPayroll.MstEmployee
                              where a.EmpID.Contains(pCode)
                              select a).FirstOrDefault();
                if (getEmp == null)
                {
                    oApplication.StatusBar.SetText("Please select Valid Employee Code", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                objAttachment = dbHrPayroll.MstEmployeeAttachment.Where(a => a.EmpId == getEmp.ID).FirstOrDefault();
                if (objAttachment == null)
                {
                    int row = oDataTable.Rows.Count - 1;
                    if (row > 0)
                    {
                        for (int i = 0; i < row; i++)
                        {
                            objAttachment = new MstEmployeeAttachment();
                            int MatEmpID = oDataTable.GetValue("EmpId", i);
                            string MatFileName = oDataTable.GetValue("FileName", i);
                            string MatFilePath = oDataTable.GetValue("ViewAttch", i);
                            objAttachment.EmpId = MatEmpID;
                            objAttachment.FileName = MatFileName;
                            objAttachment.FilePath = MatFilePath;
                            objAttachment.CreatedBy = oCompany.UserName;
                            objAttachment.CreatedDate = DateTime.Now;

                            dbHrPayroll.MstEmployeeAttachment.InsertOnSubmit(objAttachment);
                            dbHrPayroll.SubmitChanges();
                        }
                    }
                }
                else
                {
                    int row = oDataTable.Rows.Count - 1;
                    if (row > 0)
                    {
                        for (int i = 0; i < row; i++)
                        {
                            string NewRecord = oDataTable.GetValue("Check", i);
                            if (NewRecord == "New")
                            {
                                objAttachment = new MstEmployeeAttachment();
                                int MatEmpID = oDataTable.GetValue("EmpId", i);
                                string MatFileName = oDataTable.GetValue("FileName", i);
                                string MatFilePath = oDataTable.GetValue("ViewAttch", i);
                                objAttachment.EmpId = MatEmpID;
                                objAttachment.FileName = MatFileName;
                                objAttachment.FilePath = MatFilePath;
                                objAttachment.CreatedBy = oCompany.UserName;
                                objAttachment.CreatedDate = DateTime.Now;

                                dbHrPayroll.MstEmployeeAttachment.InsertOnSubmit(objAttachment);
                                dbHrPayroll.SubmitChanges();
                            }
                        }
                    }                    
                }
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("RecordSavedSuccessfully"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                oMatrix.LoadFromDataSource();
                FillGrid();
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_EmpAttch Function: InsertAttachmentRequest Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            oForm.Freeze(true);
        }

        #endregion
    }
}
