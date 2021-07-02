using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using DIHRMS.Custom;
using System.Data;
using System.IO;

namespace ACHR.Screen
{
    class frm_EmpAttAlw : HRMSBaseForm
    {
        #region Variables

        SAPbouiCOM.Button btnMain, btnCancel, btnBrowser, btnImport;
        SAPbouiCOM.EditText txtDocNum, txtFilePath;
        SAPbouiCOM.ComboBox cmbElement, cmbElementPaysThrough;
        SAPbouiCOM.CheckBox chkStatus;
        SAPbouiCOM.Matrix grdMain;
        SAPbouiCOM.DataTable dtMain;
        SAPbouiCOM.Column cEmpCode, cEmpName, cStatus, cId, cIsnew;

        SAPbouiCOM.Item itxtDocNum, itxtFilePath;

        Boolean flgValidFill = false;
        int DocID = 0;
        #endregion

        #region B1 Events

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
            switch  (pVal.ItemUID)
            {
                case "btbrowse":
                    getFileName();
                    break;
                case "btload":
                    getFileData();
                    break;
                case "1":
                    SaveRecord();
                    break;
                default:
                    break;
            }
        }

        public override void etBeforeClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etBeforeClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "1":
                    if (btnMain.Caption == "Add")
                    {
                        if (!ValidateAddRecord())
                        {
                            BubbleEvent = false;
                        }
                    }
                    if (btnMain.Caption == "Update")
                    {
                        if (!ValidateUpdateRecord())
                        {
                            BubbleEvent = false;
                        }
                    }
                    break;
            }
        }

        public override void etFormAfterActivate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etFormAfterActivate(ref pVal, ref BubbleEvent);
            if (!string.IsNullOrEmpty(Program.EmpID))
            {
                if (flgValidFill)
                    FillRecord(Program.EmpID);
            }
        }

        public override void AddNewRecord()
        {
            base.AddNewRecord();
            InitiallizeDocument();
        }

        public override void getFirstRecord()
        {
            base.getFirstRecord();
        }

        public override void getLastRecord()
        {
            base.getLastRecord();
        }

        public override void getNextRecord()
        {
            base.getNextRecord();
        }

        public override void getPreviouRecord()
        {
            base.getPreviouRecord();
        }

        public override void FindRecordMode()
        {
            base.FindRecordMode();
            InitiallizeDocument();
            OpenNewSearchWindow();
        }

        public override void fillFields()
        {
            base.fillFields();
            oForm.Freeze(true);
            FillRecord();
            oForm.Freeze(false);
        }

        #endregion

        #region Functions

        private void InitiallizeForm()
        {
            try
            {
                btnMain = oForm.Items.Item("1").Specific;
                btnCancel = oForm.Items.Item("2").Specific;
                btnBrowser = oForm.Items.Item("btbrowse").Specific;
                btnImport = oForm.Items.Item("btload").Specific;

                grdMain = oForm.Items.Item("mtmain").Specific;
                dtMain = oForm.DataSources.DataTables.Item("dtmain");
                cEmpCode = grdMain.Columns.Item("empid");
                cEmpName = grdMain.Columns.Item("empname");
                cStatus = grdMain.Columns.Item("status");
                cId = grdMain.Columns.Item("id");
                cIsnew = grdMain.Columns.Item("isnew");

                cId.Visible = false;
                cIsnew.Visible = false;
                cStatus.Visible = false;
                grdMain.AutoResizeColumns();

                txtDocNum = oForm.Items.Item("txdocnum").Specific;
                oForm.DataSources.UserDataSources.Add("txdocnum", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                itxtDocNum = oForm.Items.Item("txdocnum");
                txtDocNum.DataBind.SetBound(true, "", "txdocnum");

                txtFilePath = oForm.Items.Item("txfile").Specific;
                oForm.DataSources.UserDataSources.Add("txfile", SAPbouiCOM.BoDataType.dt_LONG_TEXT);
                itxtFilePath = oForm.Items.Item("txfile");
                txtFilePath.DataBind.SetBound(true, "", "txfile");

                chkStatus = oForm.Items.Item("chkStatus").Specific;
                oForm.DataSources.UserDataSources.Add("chkStatus", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                chkStatus.DataBind.SetBound(true, "", "chkStatus");

                cmbElement = oForm.Items.Item("cbele").Specific;
                oForm.DataSources.UserDataSources.Add("cbele", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                cmbElement.DataBind.SetBound(true, "", "cbele");

                cmbElementPaysThrough = oForm.Items.Item("cbpays").Specific;
                oForm.DataSources.UserDataSources.Add("cbpays", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                cmbElementPaysThrough.DataBind.SetBound(true, "", "cbpays");

                FillElementForCalculation(cmbElement);
                FillNonRecElementPaysThrough(cmbElementPaysThrough);
                InitiallizeDocument();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("InitiallizeForm : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void AddEmptyRow()
        {
            Int32 RowValue = 0;
            if (dtMain.Rows.Count == 0)
            {
                dtMain.Rows.Add(1);
                RowValue = dtMain.Rows.Count;
                dtMain.SetValue(cIsnew.DataBind.Alias, RowValue - 1, "Y");
                dtMain.SetValue(cId.DataBind.Alias, RowValue - 1, "0");
                dtMain.SetValue(cEmpCode.DataBind.Alias, RowValue - 1, "");
                dtMain.SetValue(cEmpName.DataBind.Alias, RowValue - 1, "");
                dtMain.SetValue(cStatus.DataBind.Alias, RowValue - 1, "Y");
                grdMain.AddRow(1, RowValue + 1);
            }
            else
            {
                if (dtMain.GetValue(cEmpCode.DataBind.Alias, dtMain.Rows.Count - 1) == "")
                {
                }
                else
                {

                    dtMain.Rows.Add(1);
                    RowValue = dtMain.Rows.Count;
                    dtMain.SetValue(cIsnew.DataBind.Alias, RowValue - 1, "Y");
                    dtMain.SetValue(cId.DataBind.Alias, RowValue - 1, "0");
                    dtMain.SetValue(cEmpCode.DataBind.Alias, RowValue - 1, "");
                    dtMain.SetValue(cEmpName.DataBind.Alias, RowValue - 1, "");
                    dtMain.SetValue(cStatus.DataBind.Alias, RowValue - 1, "Y");
                    grdMain.AddRow(1, grdMain.RowCount + 1);
                }
            }
            grdMain.LoadFromDataSource();
        }

        private void getFileName()
        {
            try
            {
                if (!string.IsNullOrEmpty(txtDocNum.Value.Trim()))
                {
                    string fileName = Program.objHrmsUI.FindFile();
                    txtFilePath.Value = fileName;
                }
                else
                {
                    oApplication.StatusBar.SetText("No employee selected.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void getFileData()
        {
            try
            {
                if (!string.IsNullOrEmpty(txtFilePath.Value.Trim()))
                {
                    string OneLine;
                    string filepath = txtFilePath.Value.Trim();
                    string[] OneLineParsed = new string[4];
                    Int16 counter = 1;
                    DataTable dtFileData = new DataTable();
                    using (StreamReader File = new StreamReader(filepath))
                    {
                        
                        dtFileData.Columns.Add("id");
                        dtFileData.Columns.Add("isnew");
                        dtFileData.Columns.Add("empcode");
                        dtFileData.Columns.Add("empname");
                        dtFileData.Columns.Add("status");
                        File.ReadLine();
                        dtFileData.Rows.Clear();
                        OneLine = File.ReadLine();
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
                                dtFileData.Rows.Add(0, "Y", OneLineParsed[0], OneLineParsed[1], "Y");
                                counter++;
                            }
                        }
                    }
                    if (dtFileData.Rows.Count > 0)
                    {
                        Int16 LineNumber = 1;
                        grdMain.Clear();
                        dtMain.Rows.Clear();
                        foreach (DataRow dr in dtFileData.Rows)
                        {
                            string empcode = txtDocNum.Value.Trim();
                            string empreffered = dr["empcode"].ToString();

                            var oValue = (from detail in dbHrPayroll.MstEmployeeReferralsDetails
                                          join head in dbHrPayroll.MstEmployeeReferrals on detail.FKID equals head.InternalID
                                          where detail.MstEmployee.EmpID == empreffered && head.MstEmployee.EmpID == empcode
                                          select new { fkempcode = head.MstEmployee.EmpID, empcode = detail.MstEmployee.EmpID }).FirstOrDefault();
                            if (oValue != null)
                            {
                                continue;
                            }
                            dtMain.Rows.Add();
                            dtMain.SetValue(cId.DataBind.Alias, LineNumber - 1, LineNumber);
                            dtMain.SetValue(cIsnew.DataBind.Alias, LineNumber - 1, "Y");
                            dtMain.SetValue(cEmpCode.DataBind.Alias, LineNumber - 1, dr["empcode"]);
                            dtMain.SetValue(cEmpName.DataBind.Alias, LineNumber - 1, dr["empname"]);
                            dtMain.SetValue(cStatus.DataBind.Alias, LineNumber - 1, "Y");
                            LineNumber++;
                        }
                        grdMain.LoadFromDataSource();
                    }
                }
                else
                {
                    oApplication.StatusBar.SetText("File didn't selected.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("getFileData : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private Boolean ValidateAddRecord()
        {
            Boolean flgValue = true;
            try
            {
                string DocNum, ElementValue;

                DocNum = txtDocNum.Value.Trim();
                if (!string.IsNullOrEmpty(DocNum))
                {
                    if (!UFFU.mFm.IsInteger(DocNum, "32"))
                    {
                        oApplication.StatusBar.SetText("Docnum should be valid.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        flgValue = false;
                    }
                }
                else
                {
                    oApplication.StatusBar.SetText("Docnum should be valid.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    flgValue = false;
                }

                ElementValue = cmbElement.Value.Trim();
                if (!string.IsNullOrEmpty(ElementValue))
                {
                    if (ElementValue == "-1")
                    {
                        oApplication.StatusBar.SetText("Element Selection is mandatory.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        flgValue = false;
                    }
                }
                else
                {
                    oApplication.StatusBar.SetText("Element Selection is mandatory.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    flgValue = false;
                }
            }
            catch (Exception ex)
            {
                flgValue = false;
            }
            return flgValue;
        }

        private Boolean ValidateUpdateRecord()
        {
            Boolean flgValue = true;
            try
            {
                string DocNum, ElementValue;

                DocNum = DocID.ToString();
                if (!string.IsNullOrEmpty(DocNum))
                {
                    if (!UFFU.mFm.IsInteger(DocNum, "32"))
                    {
                        oApplication.StatusBar.SetText("Docnum should be valid.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        flgValue = false;
                    }
                    else
                    {
                        if (DocID == 0)
                        {
                            oApplication.StatusBar.SetText("No valid document found to update.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            flgValue = false;
                        }
                    }
                }
                else
                {
                    oApplication.StatusBar.SetText("Docnum should be valid.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    flgValue = false;
                }

                ElementValue = cmbElement.Value.Trim();
                if (!string.IsNullOrEmpty(ElementValue))
                {
                }
                else
                {
                    oApplication.StatusBar.SetText("Element Selection is mandatory.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    flgValue = false;
                }
                
            }
            catch (Exception ex)
            {
                flgValue = false;
            }
            return flgValue;
        }

        private void SaveRecord()
        {
            try
            {
                string DocNum, elementID, elementIDPays;
                Boolean flgStatus = false;
                DocNum = txtDocNum.Value.Trim();
                elementID = cmbElement.Value.Trim();
                elementIDPays = cmbElementPaysThrough.Value.Trim();
                flgStatus = chkStatus.Checked;
                grdMain.FlushToDataSource();
                int chkCount = (from a in dbHrPayroll.TrnsEmployeeAttendanceAllowance where a.DocNum.ToString() == DocNum select a).Count();
                if (chkCount > 0)
                {
                    var oDoc = (from a in dbHrPayroll.TrnsEmployeeAttendanceAllowance where a.DocNum.ToString() == DocNum select a).FirstOrDefault();
                    if (oDoc != null)
                    {
                        oDoc.DocStatus = flgStatus;
                        oDoc.CalculatedOn = Convert.ToInt32(elementID);
                        oDoc.PaysThrough = Convert.ToInt32(elementIDPays);
                        oDoc.UpdatedBy = oCompany.UserName;
                        oDoc.UpdateDate = DateTime.Now;
                        for (int i = 0; i < dtMain.Rows.Count; i++)
                        {
                            string empcode;
                            empcode = dtMain.GetValue(cEmpCode.DataBind.Alias, i);
                            if (!string.IsNullOrEmpty(empcode))
                            {
                                var oLine = (from a in dbHrPayroll.TrnsEmployeeAttendanceAllowanceDetail
                                             where a.TrnsEmployeeAttendanceAllowance.DocNum.ToString() == DocNum && a.MstEmployee.EmpID == empcode
                                             select a).FirstOrDefault();
                                if (oLine != null)
                                {
                                    oLine.UpdateDate = DateTime.Now;
                                    oLine.UpdatedBy = oCompany.UserName;
                                }
                                else
                                {
                                    oLine = new TrnsEmployeeAttendanceAllowanceDetail();
                                    oDoc.TrnsEmployeeAttendanceAllowanceDetail.Add(oLine);
                                    var oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == empcode select a).FirstOrDefault();
                                    oLine.EmpID = oEmp.ID;
                                    oLine.CreatedBy = oCompany.UserName;
                                    oLine.UpdatedBy = oCompany.UserName;
                                    oLine.CreateDate = DateTime.Now;
                                    oLine.UpdateDate = DateTime.Now;
                                }
                            }
                        }
                        
                    }
                }
                else
                {
                    var oDoc = new TrnsEmployeeAttendanceAllowance();
                    dbHrPayroll.TrnsEmployeeAttendanceAllowance.InsertOnSubmit(oDoc);
                    oDoc.DocNum = Convert.ToInt32(DocNum);
                    oDoc.DocStatus = flgStatus;
                    oDoc.CalculatedOn = Convert.ToInt32(elementID);
                    oDoc.PaysThrough = Convert.ToInt32(elementIDPays);
                    oDoc.CreatedBy = oCompany.UserName;
                    oDoc.UpdatedBy = oCompany.UserName;
                    oDoc.CreateDate = DateTime.Now;
                    oDoc.UpdateDate = DateTime.Now;
                    for (int i = 0; i < dtMain.Rows.Count; i++)
                    {
                        string empcode;
                        empcode = dtMain.GetValue(cEmpCode.DataBind.Alias, i);
                        if (!string.IsNullOrEmpty(empcode))
                        {
                            var oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == empcode select a).FirstOrDefault();
                            if (oEmp == null)
                            {
                                oApplication.StatusBar.SetText("Emp ID : " + empcode + " was not a valid employee code.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                continue;
                            }
                            var oLine = new TrnsEmployeeAttendanceAllowanceDetail();
                            oDoc.TrnsEmployeeAttendanceAllowanceDetail.Add(oLine);
                            oLine.EmpID = oEmp.ID;
                            oLine.CreatedBy = oCompany.UserName;
                            oLine.UpdatedBy = oCompany.UserName;
                            oLine.CreateDate = DateTime.Now;
                            oLine.UpdateDate = DateTime.Now;
                        }
                    }
                }
                dbHrPayroll.SubmitChanges();
                oApplication.StatusBar.SetText("Document Successfully Saved.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                InitiallizeDocument();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("SaveRecord : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void ClearRecord()
        {
            try
            {
                string docnum = "";
                var oValue = (from a in dbHrPayroll.TrnsEmployeeAttendanceAllowance select a).Max(a => a.DocNum).GetValueOrDefault();
                if (oValue != null)
                {
                    docnum = (oValue + 1).ToString();
                }
                else
                {
                    docnum = "1";
                }
                txtDocNum.Value = docnum;
                txtFilePath.Value = "";
                cmbElementPaysThrough.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cmbElement.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                chkStatus.Checked = true;
                dtMain.Rows.Clear();
                AddEmptyRow();
                
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("ClearRecord : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillRecord()
        {
            try
            {
                if (CodeIndex.Count == 0) return;
                string value = CodeIndex[currentRecord].ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    var oDoc = (from a in dbHrPayroll.TrnsEmployeeAttendanceAllowance
                                where a.InternalID.ToString() == value
                                select a).FirstOrDefault();
                    if (oDoc == null) return;
                    DocID = oDoc.InternalID;
                    txtDocNum.Value = oDoc.DocNum.ToString();
                    chkStatus.Checked = Convert.ToBoolean(oDoc.DocStatus);
                    cmbElement.Select(oDoc.CalculatedOn != null ? Convert.ToString(oDoc.CalculatedOn) : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                    cmbElementPaysThrough.Select(oDoc.PaysThrough != null ? Convert.ToString(oDoc.PaysThrough) : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                    txtFilePath.Value = "";
                    int i = 0;
                    if (oDoc.TrnsEmployeeAttendanceAllowanceDetail.Count > 0) dtMain.Rows.Clear();
                    foreach (var oLine in oDoc.TrnsEmployeeAttendanceAllowanceDetail)
                    {
                        dtMain.Rows.Add(1);
                        dtMain.SetValue(cId.DataBind.Alias, i, oLine.InternalID);
                        dtMain.SetValue(cIsnew.DataBind.Alias, i, "N");
                        dtMain.SetValue(cEmpCode.DataBind.Alias, i, oLine.MstEmployee.EmpID);
                        dtMain.SetValue(cEmpName.DataBind.Alias, i, oLine.MstEmployee.FirstName + " " + oLine.MstEmployee.MiddleName + " " + oLine.MstEmployee.LastName);
                        dtMain.SetValue(cStatus.DataBind.Alias, i, "Y");
                        i++;
                    }
                    
                }
                AddEmptyRow();
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("fill record : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillRecord(string docid)
        {
            try
            {
                if (!string.IsNullOrEmpty(docid))
                {
                    var oDoc = (from a in dbHrPayroll.TrnsEmployeeAttendanceAllowance
                                where a.InternalID.ToString() == docid
                                select a).FirstOrDefault();
                    if (oDoc == null) return;
                    DocID = oDoc.InternalID;
                    txtDocNum.Value = oDoc.DocNum.ToString();
                    chkStatus.Checked = Convert.ToBoolean(oDoc.DocStatus);
                    cmbElement.Select(oDoc.CalculatedOn != null ? Convert.ToString(oDoc.CalculatedOn) : "-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                    txtFilePath.Value = "";
                    int i = 0;
                    if (oDoc.TrnsEmployeeAttendanceAllowanceDetail.Count > 0) dtMain.Rows.Clear();
                    foreach (var oLine in oDoc.TrnsEmployeeAttendanceAllowanceDetail)
                    {
                        dtMain.Rows.Add(1);
                        dtMain.SetValue(cId.DataBind.Alias, i, oLine.InternalID);
                        dtMain.SetValue(cIsnew.DataBind.Alias, i, "N");
                        dtMain.SetValue(cEmpCode.DataBind.Alias, i, oLine.MstEmployee.EmpID);
                        dtMain.SetValue(cEmpName.DataBind.Alias, i, oLine.MstEmployee.FirstName + " " + oLine.MstEmployee.MiddleName + " " + oLine.MstEmployee.LastName);
                        dtMain.SetValue(cStatus.DataBind.Alias, i, "Y");
                        i++;
                    }

                }
                AddEmptyRow();
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("fill record : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillElementForCalculation(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                var oCollection = (from Head in dbHrPayroll.MstElements
                                   join Detail in dbHrPayroll.MstElementEarning on Head.Id equals Detail.ElementID
                                   //where Head.Type == "Non-Rec" && Head.ElmtType == "Ear" && Detail.ValueType == "FIX"
                                   where Head.Type != "Non-Rec" && Head.ElmtType == "Ear" && Detail.ValueType != "FIX"
                                   select new { Id = Head.Id, ElementName = Head.ElementName }).ToList();
                pCombo.ValidValues.Add("-1", "Select Element");
                if (oCollection.Count > 0)
                {
                    foreach (var One in oCollection)
                    {
                        pCombo.ValidValues.Add(Convert.ToString(One.Id), Convert.ToString(One.ElementName));
                    }
                }
                pCombo.Select("-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("FillNonRecElement : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillNonRecElementPaysThrough(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                var oCollection = (from Head in dbHrPayroll.MstElements
                                   join Detail in dbHrPayroll.MstElementEarning on Head.Id equals Detail.ElementID
                                   where Head.Type == "Non-Rec" && Head.ElmtType == "Ear" && Detail.ValueType == "FIX"
                                   select new { Id = Head.Id, ElementName = Head.ElementName }).ToList();
                pCombo.ValidValues.Add("-1", "Select Element");
                if (oCollection.Count > 0)
                {
                    foreach (var One in oCollection)
                    {
                        pCombo.ValidValues.Add(Convert.ToString(One.Id), Convert.ToString(One.ElementName));
                    }
                }
                pCombo.Select("-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("FillNonRecElement : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void GetData()
        {
            try
            {
                CodeIndex.Clear();
                var oDocuments = (from a in dbHrPayroll.TrnsEmployeeAttendanceAllowance select a).ToList();
                Int32 i = 0;
                foreach (var oDoc in oDocuments)
                {
                    CodeIndex.Add(i, oDoc.InternalID);
                    i++;
                }
                totalRecord = i;
            }
            catch (Exception ex)
            {
            }
        }

        private void InitiallizeDocument()
        {
            try
            {
                ClearRecord();
                GetData();
                grdMain.AutoResizeColumns();
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            }
            catch(Exception ex)
            {
            }
        }

        private void OpenNewSearchWindow()
        {
            try
            {
                InitiallizeDocument();
                flgValidFill = true;
                Program.EmpID = "";
                string comName = "MstSearchEAS";
                Program.sqlString = "";
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

        #endregion 

    }
}
