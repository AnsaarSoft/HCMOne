using System;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DIHRMS;
using DIHRMS.Custom;
using SAPbobsCOM;
using SAPbouiCOM;

namespace ACHR.Screen
{
    class frm_switch : HRMSBaseForm
    {
        #region Variable

        SAPbouiCOM.Button btnSwitch, btnOk;
        SAPbouiCOM.EditText txtSelectedDb;
        SAPbouiCOM.Matrix mtMain;
        SAPbouiCOM.DataTable dtMain;
        SAPbouiCOM.Column clCode, clName;
        SAPbouiCOM.Item itxtSelectedDb, ibtnOK, ibtnSwitch;

        private dbHRMS dbHr;

        #endregion 

        #region SAPB1 Events

        public override void CreateForm(Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.Freeze(true);
            InitillizeForm();
            //IbtnOK.Visible = false;
            oForm.Freeze(false);
        }

        public override void etAfterClick(ref ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            if (pVal.ItemUID == "mtmain" && !pVal.BeforeAction)
            {
                int rowNum = pVal.Row;
                if (rowNum == 0)
                {
                    txtSelectedDb.Value = "";
                    ibtnSwitch.Enabled = false;
                }
                if (rowNum > 0)
                {
                    if (rowNum <= dtMain.Rows.Count)
                    {
                        txtSelectedDb.Value = dtMain.GetValue(clCode.DataBind.Alias, rowNum - 1);
                        ibtnSwitch.Enabled = true;
                    }
                }
            }
            if (pVal.ItemUID == "btdo")
            {
                SwitchCompany();
            }
        }

        #endregion 

        #region Function 

        private void InitillizeForm()
        {
            try
            {
                btnSwitch = oForm.Items.Item("btdo").Specific;
                ibtnSwitch = oForm.Items.Item("btdo");
                btnOk = oForm.Items.Item("1").Specific;
                ibtnOK = oForm.Items.Item("1");

                oForm.DataSources.UserDataSources.Add("txdb", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txtSelectedDb = oForm.Items.Item("txdb").Specific;
                itxtSelectedDb = oForm.Items.Item("txdb");
                txtSelectedDb.DataBind.SetBound(true, "", "txdb");

                mtMain = oForm.Items.Item("mtmain").Specific;
                dtMain = oForm.DataSources.DataTables.Item("dtmain");
                clCode = mtMain.Columns.Item("clcode");
                clName = mtMain.Columns.Item("clname");
                mtMain.AutoResizeColumns();
                LoadData();
                ibtnSwitch.Enabled = false;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("InitillizeForm : " + ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }
        }

        private void LoadData()
        {
            try
            {
                System.Data.DataTable dtAchrData = new System.Data.DataTable();
                dtAchrData.Columns.Add("Code");
                dtAchrData.Columns.Add("Description");

                SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecSet.DoQuery("SELECT * FROM \"@ACHR_CONFIG\"");
                while (oRecSet.EoF == false)
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(oRecSet.Fields.Item("Code").Value)))
                    {
                        string ostrcode = Convert.ToString(oRecSet.Fields.Item("Code").Value);
                        string ostrname = Convert.ToString(oRecSet.Fields.Item("Name").Value);
                        dtAchrData.Rows.Add(ostrcode.Trim(), ostrname.Trim());
                    }
                    oRecSet.MoveNext();
                }
                
                //string strquery = @"SELECT A2.U_COMCODE Code, A2.U_COMSTS CompanyStatus FROM dbo.[@DBA] A1 INNER JOIN dbo.[@DBAD] A2 ON A1.DocEntry = A2.DocEntry INNER JOIN dbo.OUSR A3 ON A3.U_Companies = A1.DocNum WHERE A3.USER_CODE = '" + oCompany.UserName + "'";
                string strquery = "SELECT A2.\"U_COMCODE\" as  Code, A2.\"U_COMSTS\" as  CompanyStatus FROM \"@DBA\" A1 INNER JOIN \"@DBAD\" A2 ON A1.\"DocEntry\" = A2.\"DocEntry\" INNER JOIN OUSR A3 ON A3.\"U_Companies\" = A1.\"DocNum\" WHERE A3.\"USER_CODE\" = '" + oCompany.UserName + "'";
                SAPbobsCOM.Recordset oRecSet1 = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecSet1.DoQuery(strquery);
                while (oRecSet1.EoF == false)
                {
                    string filtercode = Convert.ToString(oRecSet1.Fields.Item("Code").Value);
                    string filterstatus = Convert.ToString(oRecSet1.Fields.Item("CompanyStatus").Value);
                    if (!string.IsNullOrEmpty(filterstatus))
                    {
                        if (filterstatus.Trim().ToLower() != "y")
                        {
                            for (int i = 0; i < dtAchrData.Rows.Count; i++)
                            {
                                if (dtAchrData.Rows[i][0].ToString() == filtercode)
                                {
                                    dtAchrData.Rows[i].Delete();
                                    //dtAchrData.Rows.RemoveAt(dtAchrData.Rows.IndexOf(dtrow));
                                }
                            }

                        }
                    }
                    oRecSet1.MoveNext();
                }

                if (dtAchrData.Rows.Count > 0)
                {
                    dtMain.Rows.Clear();
                    for (int i = 0; i < dtAchrData.Rows.Count; i++)
                    {
                        dtMain.Rows.Add();
                        dtMain.SetValue(clCode.DataBind.Alias, i, dtAchrData.Rows[i][0].ToString());
                        dtMain.SetValue(clName.DataBind.Alias, i, dtAchrData.Rows[i][1].ToString());
                    }
                    mtMain.LoadFromDataSource();
                }
            }
            catch(Exception ex)
            {
                oApplication.StatusBar.SetText("LoadData : " + ex.Message);
            }
        }

        private void SwitchCompany()
        {
            try
            {
                int confirm = oApplication.MessageBox("Are you sure you want to Login into " + txtSelectedDb.Value.Trim(), 3, "Yes", "No", "Cancel");
                if (confirm == 2 || confirm == 3)
                {
                    return;
                }

                string comcode = string.Empty;
                string strOut = string.Empty;
                if (!string.IsNullOrEmpty(txtSelectedDb.Value))
                {
                    comcode = txtSelectedDb.Value.Trim();
                }
                else
                {
                    return;
                }
                Program.systemInfo = null;
                Program.ConStrHRMS = string.Empty;
                SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecSet.DoQuery("SELECT *  FROM \"@ACHR_CONFIG\" WHERE \"Code\" = '" + comcode + "'");
                if (oRecSet.EoF)
                {
                    oApplication.StatusBar.SetText("Configure HRMS before further proceeding!", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                    return;
                }
                strOut = oRecSet.Fields.Item("u_server").Value;
                strOut = "Data Source=" + oRecSet.Fields.Item("u_server").Value + ";Initial Catalog=" + oRecSet.Fields.Item("U_db").Value + ";User ID=" + oRecSet.Fields.Item("U_uid").Value + ";Password=" + oRecSet.Fields.Item("U_pwd").Value + ";" + "MultipleActiveResultSets=True";
                Program.ConStrHRMS = strOut;
                Program.objHrmsUI.hrConstr = strOut;
                Program.objHrmsUI.HRMSDbName = oRecSet.Fields.Item("U_db").Value;
                Program.objHrmsUI.HRMSDbServer = oRecSet.Fields.Item("U_server").Value;
                Program.objHrmsUI.HRMSDBuid = oRecSet.Fields.Item("U_uid").Value;
                Program.objHrmsUI.HRMSDbPwd = oRecSet.Fields.Item("U_pwd").Value;
                Program.objHrmsUI.HRMServerType = oRecSet.Fields.Item("U_SvrType").Value;
                Program.objHrmsUI.HRMSLicHash = oRecSet.Fields.Item("U_LicKey").Value;
                Program.objHrmsUI.BranchName = oRecSet.Fields.Item("U_BranchName").Value;
                Program.objHrmsUI.JeSeries = oRecSet.Fields.Item("U_JES").Value;
                dbHr = new dbHRMS(Program.ConStrHRMS);
                Program.systemInfo = (from a in dbHr.CfgPayrollBasicInitialization select a).FirstOrDefault();
                Program.objHrmsUI.dbHr = new dbHRMS(Program.ConStrHRMS);
                oApplication.StatusBar.SetText("Successfully switch to Company : " + txtSelectedDb.Value.Trim(), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                oForm.Close();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("SwitchCompany : " + ex.Message);
            }
        }

        #endregion 

    }
}
