using DIHRMS;
using DIHRMS.Custom;
using System;
using System.Data;
using System.Linq;

namespace ACHR.Screen
{
    class frm_mATT : HRMSBaseForm
    {
        #region "Global Variable & objects"
        //Form all Element Objects
        SAPbouiCOM.Button btnAdd, btnCancel, btId;

        SAPbouiCOM.EditText txtEmpC, txtEmpN, txtDate, txtTime;
        SAPbouiCOM.Item itxtDate, itxtTime, icmbInO, ItxtEmpC, ItxtEmpN, ItxtDate, ItxtTime;
        SAPbouiCOM.ComboBox cmbInO;

        #endregion

        #region "Events"

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            //Assign Object to Form Elements.
            try
            {
                btnAdd = oForm.Items.Item("1").Specific;
                btnCancel = oForm.Items.Item("2").Specific;
                btId = oForm.Items.Item("btId").Specific;

                txtEmpC = oForm.Items.Item("txtEmpC").Specific;
                oForm.DataSources.UserDataSources.Add("txtEmpC", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txtEmpC.DataBind.SetBound(true, "", "txtEmpC");
                ItxtEmpC = oForm.Items.Item("txtEmpC");


                txtEmpN = oForm.Items.Item("txtEmpN").Specific;
                oForm.DataSources.UserDataSources.Add("txtEmpN", SAPbouiCOM.BoDataType.dt_LONG_TEXT, 150);
                txtEmpN.DataBind.SetBound(true, "", "txtEmpN");
                ItxtEmpN = oForm.Items.Item("txtEmpN");

                oForm.DataSources.UserDataSources.Add("txtDate", SAPbouiCOM.BoDataType.dt_DATE, 30);
                itxtDate = oForm.Items.Item("txtDate");
                txtDate = oForm.Items.Item("txtDate").Specific;
                txtDate.DataBind.SetBound(true, "", "txtDate");

                itxtDate.Enabled = true;

                txtTime = oForm.Items.Item("txtTime").Specific;
                oForm.DataSources.UserDataSources.Add("txtTime", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtTime.DataBind.SetBound(true, "", "txtTime");
                itxtTime = oForm.Items.Item("txtTime");
                itxtTime.Enabled = true;

                //cmbInO = oForm.Items.Item("cmbInO").Specific;
                cmbInO = oForm.Items.Item("cmbInO").Specific;
                oForm.DataSources.UserDataSources.Add("cmbInO", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cmbInO.DataBind.SetBound(true, "", "cmbInO");
                icmbInO = oForm.Items.Item("cmbInO");



                FillInOutCombo();

                //SetFieldsAuthorization();
            }
            catch (Exception Ex)
            {
                oApplication.SetStatusBarMessage(Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, true);
                //Program.objHrmsUI.getStrMsg("xception");
            }


        }
        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            try
            {
                switch (pVal.ItemUID)
                {
                    case "1":
                        SaveRecord();
                        break;
                    case "btId":
                        OpenNewSearchForm();
                        break;
                    case "2":
                        break;
                    default:
                        break;
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_AdvncReq Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        public override void etFormAfterActivate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etFormAfterActivate(ref pVal, ref BubbleEvent);
            if (txtEmpC == null)
            {
                Program.EmpID = string.Empty;
                return;
            }
            if (Program.EmpID == string.Empty)
            {
                return;
            }
            if (Program.EmpID == txtEmpC.Value)
            {
                return;
            }
            else
            {
                SetEmpValues();
            }


        }
        public override void FindRecordMode()
        {
            base.FindRecordMode();

            OpenNewSearchForm();

            oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;

        }

        #endregion

        #region Helper Methods


        private void FillInOutCombo()
        {
            try
            {
                //cmbInO.ValidValues.Add("-1", "[Select One]");
                cmbInO.ValidValues.Add("1", "[IN]");
                cmbInO.ValidValues.Add("2", "[OUT]");

                //var Data = from v in dbHrPayroll.MstAdvance where v.FlgActive == true select v;
                //foreach (var v in Data)
                //{
                //    cmbInO.ValidValues.Add(v.Id.ToString(), v.Description);
                //}
                cmbInO.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void OpenNewSearchForm()
        {
            try
            {
                Program.EmpID = "";
                //string comName = "Search";
                //Program.sqlString = "empAdvance";
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

                }

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void SetEmpValues()
        {
            try
            {
                if (!string.IsNullOrEmpty(Program.EmpID))
                {
                    txtEmpC.Value = Program.EmpID;
                    LoadSelectedData(txtEmpC.Value);
                    //oForm.ActiveItem = "txtDate";
                    Program.EmpID = "";
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void LoadSelectedData(String pCode)
        {

            try
            {

                if (!String.IsNullOrEmpty(pCode))
                {
                    var getEmp = (from a in dbHrPayroll.MstEmployee
                                  where a.EmpID == pCode
                                  select a).FirstOrDefault();
                    if (getEmp != null)
                    {
                        txtEmpN.Value = getEmp.FirstName + " " + getEmp.MiddleName + " " + getEmp.LastName;
                        txtDate.Value = DateTime.Now.ToString("yyyyMMdd");
                        var timeZone = TimeZoneInfo.GetSystemTimeZones();
                        //var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                        txtTime.Value = DateTime.Now.ToString("HH:mm");
                        oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                    }

                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_AdvncReq Function: LoadSelectedData Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void SaveRecord()
        {
            try
            {
                DateTime startDate = DateTime.MinValue;

                var oEmp = (from a in dbHrPayroll.MstEmployee
                            where a.EmpID == Convert.ToString(txtEmpC.Value)
                            && a.FlgActive == true
                            select a).FirstOrDefault();
                if (oEmp != null)
                {
                    startDate = DateTime.ParseExact(txtDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    var AttendanceAlreadyProcessed = (from a in dbHrPayroll.TrnsAttendanceRegister
                                                      where a.Date == startDate
                                                      && a.EmpID == oEmp.ID
                                                      && a.Processed.GetValueOrDefault() == true
                                                      select a).Count();
                    if (AttendanceAlreadyProcessed == 1)
                    {
                        oApplication.StatusBar.SetText("Employee ID '" + txtEmpC.Value.Trim() + "' already processed on '" + startDate + "'", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return;
                    }

                    TrnsTempAttendance objTempAttendance = new TrnsTempAttendance();
                    dbHrPayroll.TrnsTempAttendance.InsertOnSubmit(objTempAttendance);
                    objTempAttendance.EmpID = Convert.ToString(txtEmpC.Value);
                    objTempAttendance.In_Out = cmbInO.Selected.Value;
                    objTempAttendance.PunchedDate = DateTime.ParseExact(txtDate.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    objTempAttendance.PunchedTime = txtTime.Value.Trim();
                    objTempAttendance.CreatedDate = DateTime.Now;
                    objTempAttendance.UserID = oCompany.UserName;
                    //objTempAttendance.FlgSmsSend = false;

                    dbHrPayroll.SubmitChanges();
                    ClearControls();
                    oApplication.StatusBar.SetText("Record Saved Successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_AdvncReq Function: SaveRecord Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void ClearControls()
        {
            txtEmpC.Value = string.Empty;
            txtDate.Value = string.Empty;

            txtEmpN.Value = string.Empty;
            txtTime.Value = string.Empty;
            cmbInO.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
        }
        private void SetFieldsAuthorization()
        {
            try
            {
                string strSql = "select USERID,user_code , U_NAME, ISNULL(U_ManATT,0) AS U_ManATT from " + oCompany.CompanyDB + ".dbo.ousr  WHERE USER_CODE='" + oCompany.UserName + "' ";
                DataTable dt = ds.getDataTable(strSql);
                if (dt != null && dt.Rows.Count > 0)
                {
                    string strManualAttendance = dt.Rows[0]["U_ManATT"].ToString();
                    if (!string.IsNullOrEmpty(strManualAttendance) && strManualAttendance == "1")
                    {
                        itxtDate.Enabled = true;
                        itxtTime.Enabled = true;
                    }
                    else
                    {
                        itxtDate.Enabled = false;
                        itxtTime.Enabled = false;
                    }
                }

            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_mATT Function: SetFieldsAuthorization Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        #endregion

    }
}
