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
    class frm_EmpRef : HRMSBaseForm
    {
        #region Variables

        SAPbouiCOM.Button btnMain, btnCancel, btnBrowser, btnImport, btnEmpPick;
        SAPbouiCOM.EditText txtEmpCode, txtEmpName, txtFilePath, txtRefCount;
        SAPbouiCOM.Matrix grdMain;
        SAPbouiCOM.DataTable dtMain;
        SAPbouiCOM.Column cEmpCode, cEmpName, cStatus, cId, cIsnew;

        SAPbouiCOM.Item itxtEmpCode, itxtEmpName, itxtFilePath, itxtRefCount;

        Boolean flgValidFill = false;

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
            switch (pVal.ItemUID)
            {
                case "btemp":
                    flgValidFill = true;
                    OpenNewSearchForm();
                    break;
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

        public override void etFormAfterActivate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etFormAfterActivate(ref pVal, ref BubbleEvent);
            if (flgValidFill)
            {
                FillEmployeeRecord(Program.EmpID);
            }
        }
        public override void FindRecordMode()
        {
            base.FindRecordMode();

            OpenNewSearchForm();

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
                btnEmpPick = oForm.Items.Item("btemp").Specific;

                grdMain = oForm.Items.Item("mtmain").Specific;
                dtMain = oForm.DataSources.DataTables.Item("dtmain");
                cEmpCode = grdMain.Columns.Item("empid");
                cEmpName = grdMain.Columns.Item("empname");
                cStatus = grdMain.Columns.Item("status");
                cId = grdMain.Columns.Item("id");
                cIsnew = grdMain.Columns.Item("isnew");

                cId.Visible = false;
                cIsnew.Visible = false;

                grdMain.AutoResizeColumns();

                txtEmpCode = oForm.Items.Item("txcode").Specific;
                oForm.DataSources.UserDataSources.Add("txcode", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                itxtEmpCode = oForm.Items.Item("txcode");
                txtEmpCode.DataBind.SetBound(true, "", "txcode");

                txtEmpName = oForm.Items.Item("txempn").Specific;
                oForm.DataSources.UserDataSources.Add("txempn", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                itxtEmpName = oForm.Items.Item("txempn");
                txtEmpName.DataBind.SetBound(true, "", "txempn");

                txtFilePath = oForm.Items.Item("txfile").Specific;
                oForm.DataSources.UserDataSources.Add("txfile", SAPbouiCOM.BoDataType.dt_LONG_TEXT);
                itxtFilePath = oForm.Items.Item("txfile");
                txtFilePath.DataBind.SetBound(true, "", "txfile");

                txtRefCount = oForm.Items.Item("txref").Specific;
                oForm.DataSources.UserDataSources.Add("txref", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                itxtRefCount = oForm.Items.Item("txref");
                txtRefCount.DataBind.SetBound(true, "", "txref");
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("InitiallizeForm : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void OpenNewSearchForm()
        {
            try
            {
                Program.EmpID = "";
                string comName = "MstSearch";
                Program.sqlString = "empPick";
                //Program.sqlString = "empMaster";
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

        private void FillEmployeeRecord(string pempid)
        {
            try
            {
                if (!string.IsNullOrEmpty(pempid))
                {
                    var oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == pempid select a).FirstOrDefault();
                    if (oEmp != null)
                    {
                        txtEmpCode.Value = oEmp.EmpID;
                        txtEmpName.Value = oEmp.FirstName + " " + oEmp.MiddleName + " " + oEmp.LastName;

                        var oEmpRef = (from a in dbHrPayroll.MstEmployeeReferrals where a.EmpID == oEmp.ID select a).FirstOrDefault();
                        if (oEmpRef != null)
                        {
                            txtRefCount.Value = oEmpRef.MstEmployeeReferralsDetails.Count.ToString();
                            int i = 0;
                            foreach (var One in oEmpRef.MstEmployeeReferralsDetails)
                            {
                                dtMain.Rows.Add(1);
                                dtMain.SetValue(cId.DataBind.Alias, i, One.InternalID);
                                dtMain.SetValue(cIsnew.DataBind.Alias, i, "N");
                                dtMain.SetValue(cEmpCode.DataBind.Alias, i, One.MstEmployee.EmpID);
                                dtMain.SetValue(cEmpName.DataBind.Alias, i, One.MstEmployee.FirstName + " " + One.MstEmployee.MiddleName + " " + One.MstEmployee.LastName);
                                dtMain.SetValue(cStatus.DataBind.Alias, i, (One.FlgActive != null ? One.FlgActive : false) == true ? "Y" : "N");
                                i++;
                            }
                            AddEmptyRow();
                            oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                        }
                        else
                        {
                            oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                            dtMain.Rows.Clear();
                            grdMain.LoadFromDataSource();
                        }
                        AddEmptyRow();
                    }
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("FillEmployeeRecord : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            flgValidFill = false;
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
                if (!string.IsNullOrEmpty(txtEmpCode.Value.Trim()))
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
                            string empcode = txtEmpCode.Value.Trim();
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

        private void SaveRecord()
        {
            try
            {
                string MainEmployee = txtEmpCode.Value.Trim();
                if (!string.IsNullOrEmpty(MainEmployee))
                {
                    var oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == MainEmployee select a).FirstOrDefault();
                    int CheckCount = (from a in dbHrPayroll.MstEmployeeReferrals where a.MstEmployee.EmpID == MainEmployee select a).Count();
                    if (CheckCount == 0)
                    {
                        MstEmployeeReferrals oDoc = new MstEmployeeReferrals();
                        oDoc.MstEmployee = oEmp;
                        oDoc.ReferralCounts = 0;
                        oDoc.CreateDate = DateTime.Now;
                        oDoc.UpdateDate = DateTime.Now;
                        oDoc.CreatedBy = oCompany.UserName;
                        oDoc.UpdatedBy = oCompany.UserName;
                        for (int i = 0; i < dtMain.Rows.Count; i++)
                        {
                            string empcode = dtMain.GetValue(cEmpCode.DataBind.Alias, i);
                            if (!string.IsNullOrEmpty(empcode))
                            {
                                var oEmpRef = (from a in dbHrPayroll.MstEmployee where a.EmpID == empcode select a).FirstOrDefault();
                                if (oEmpRef != null)
                                {
                                    MstEmployeeReferralsDetails oDetail = new MstEmployeeReferralsDetails();
                                    oDetail.MstEmployee = oEmpRef;
                                    oDetail.FlgActive = true;
                                    oDoc.MstEmployeeReferralsDetails.Add(oDetail);
                                }
                                else
                                {
                                    oApplication.StatusBar.SetText("Employee not found : " + empcode, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    continue;
                                }
                            }
                        }
                        dbHrPayroll.SubmitChanges();
                        ClearRecord();
                    }
                    else
                    {
                        var oDoc = (from a in dbHrPayroll.MstEmployeeReferrals where a.MstEmployee.EmpID == MainEmployee select a).FirstOrDefault();
                        oDoc.UpdatedBy = oCompany.UserName;
                        oDoc.UpdateDate = DateTime.Now;
                        grdMain.FlushToDataSource();
                        for (int i = 0; i < dtMain.Rows.Count; i++)
                        {
                            int vid = dtMain.GetValue(cId.DataBind.Alias, i);
                            string visnew = dtMain.GetValue(cIsnew.DataBind.Alias, i);
                            string vempcode = dtMain.GetValue(cEmpCode.DataBind.Alias, i);
                            string vstatus = dtMain.GetValue(cStatus.DataBind.Alias, i);
                            if (!string.IsNullOrEmpty(vempcode))
                            {
                                if (visnew.ToLower() == "y")
                                {
                                    var oEmpRef = (from a in dbHrPayroll.MstEmployee where a.EmpID == vempcode select a).FirstOrDefault();
                                    if (oEmpRef != null)
                                    {
                                        MstEmployeeReferralsDetails oDetail = new MstEmployeeReferralsDetails();
                                        oDetail.MstEmployee = oEmpRef;
                                        oDetail.FlgActive = true;
                                        oDoc.MstEmployeeReferralsDetails.Add(oDetail);
                                    }
                                    else
                                    {
                                        oApplication.StatusBar.SetText("Employee not found : " + vempcode, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                        continue;
                                    }
                                }
                                else
                                {
                                    var oEmpRef = (from a in dbHrPayroll.MstEmployee where a.EmpID == vempcode select a).FirstOrDefault();
                                    if (oEmpRef != null)
                                    {
                                        MstEmployeeReferralsDetails oDetail = (from a in dbHrPayroll.MstEmployeeReferralsDetails where a.InternalID == vid select a).FirstOrDefault();
                                        oDetail.FlgActive = vstatus.ToLower() == "y" ? true : false;
                                    }
                                }
                            }
                            else
                            {
                                continue;
                            }
                        }
                        dbHrPayroll.SubmitChanges();
                        ClearRecord();
                    }
                }
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
                txtEmpCode.Value = "";
                txtEmpName.Value = "";
                txtRefCount.Value = "";
                txtFilePath.Value = "";
                dtMain.Rows.Clear();
                AddEmptyRow();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("ClearRecord : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        #endregion 

    }
}
