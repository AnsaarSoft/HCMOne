using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using DIHRMS.Custom;
using System.Data;

namespace ACHR.Screen
{
    class frm_OTSlabs : HRMSBaseForm
    {

        #region Variables

        SAPbouiCOM.EditText txtCode;
        SAPbouiCOM.Item itxtCode;
        SAPbouiCOM.DataTable dtMain;
        SAPbouiCOM.Matrix grdMain;
        SAPbouiCOM.Column clID, clIsNew, clOTCode, clOTDesc, clLowerLimit, clUpperLimit, clPriority, clPicker;
        SAPbouiCOM.Button btnMain, btnCancel;


        Boolean flgValidCall = false, flgDocMode = false;

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
            if (pVal.Before_Action == false)
            {
                switch (pVal.ItemUID)
                {
                    case "1":
                        SaveRecord();
                        break;
                    case "mtMain":
                        if (pVal.ColUID == "clpicker")
                        {
                            OTPicker(pVal.Row - 1);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public override void etBeforeClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etBeforeClick(ref pVal, ref BubbleEvent);
            try
            {
                switch (pVal.ItemUID)
                {
                    case "1":
                        if (btnMain.Caption == "Update")
                        {
                            if (flgDocMode)
                            {
                                if (ValidateUpdateRecord())
                                {
                                }
                                else
                                {
                                    BubbleEvent = false;
                                }
                            }
                            else
                            {

                            }
                        }
                        else if (btnMain.Caption == "Add")
                        {
                            if (ValidateAddRecord())
                            {
                            }
                            else
                            {
                                BubbleEvent = false;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            catch
            {
            }
        }

        public override void etAfterLostFocus(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterLostFocus(ref pVal, ref BubbleEvent);
            if (pVal.ItemUID == "mtMain" && pVal.ColUID == "clpr")
            {
                oForm.Freeze(true);
                grdMain.FlushToDataSource();
                AddEmptyRow();
                oForm.Freeze(false);
            }
        }

        public override void etFormAfterActivate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etFormAfterActivate(ref pVal, ref BubbleEvent);
            if (!string.IsNullOrEmpty(Program.EmpID))
            {
                if (flgValidCall)
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
            flgDocMode = true;
            FillRecord();
            oForm.Freeze(false);
        }

        #endregion

        #region Functions

        private void InitiallizeForm()
        {
            try
            {
                Program.EmpID = "";
                txtCode = oForm.Items.Item("txCode").Specific;
                oForm.DataSources.UserDataSources.Add("txCode", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                itxtCode = oForm.Items.Item("txCode");
                txtCode.DataBind.SetBound(true, "", "txCode");
                txtCode.TabOrder = 1;

                grdMain = oForm.Items.Item("mtMain").Specific;
                dtMain = oForm.DataSources.DataTables.Item("dtMain");
                
                clID = grdMain.Columns.Item("clid");
                clID.Visible = false;
                clIsNew = grdMain.Columns.Item("clisnew");
                clIsNew.Visible = false;
                clOTCode = grdMain.Columns.Item("clottype");
                clOTDesc = grdMain.Columns.Item("clotdesc");
                clPicker = grdMain.Columns.Item("clpicker");
                clLowerLimit = grdMain.Columns.Item("clll");
                clUpperLimit = grdMain.Columns.Item("clul");
                clPriority = grdMain.Columns.Item("clpr");

                btnMain = oForm.Items.Item("1").Specific;
                btnCancel = oForm.Items.Item("2").Specific;
                
                InitiallizeDocument();
                GetData();
                grdMain.AutoResizeColumns();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("InitiallizeForm : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void InitiallizeDocument()
        {
            try
            {
                flgDocMode = false;
                txtCode.Value = "";
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                dtMain.Rows.Clear();
                AddEmptyRow();
                itxtCode.Click();
                //oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("InitiallizeDocument : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private Boolean ValidateAddRecord()
        {
            try
            {
                //checking for duplicate code

                string tempvalue = txtCode.Value.Trim();
                var oDoc = (from a in dbHrPayroll.TrnsOTSlab where a.SlabCode.ToLower() == tempvalue.ToLower() select a).FirstOrDefault();
                if (oDoc != null)
                {
                    oApplication.StatusBar.SetText("Define code already in use.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }

                
                //Negate value not supported.
                grdMain.FlushToDataSource();
                for (int i = 0; i < dtMain.Rows.Count; i++)
                {
                    double lowerlimit = 0, upperlimit = 0;
                    int priority = 0;
                    lowerlimit = Convert.ToDouble(dtMain.GetValue(clLowerLimit.DataBind.Alias, i));
                    upperlimit = Convert.ToDouble(dtMain.GetValue(clUpperLimit.DataBind.Alias, i));
                    upperlimit = Convert.ToDouble(dtMain.GetValue(clUpperLimit.DataBind.Alias, i));
                    if ((lowerlimit < 0) || (upperlimit < 0))
                    {
                        oApplication.StatusBar.SetText("Negative not supported. @ Line # " + (i+1).ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return false;
                    }
                    if (lowerlimit > upperlimit)
                    {
                        oApplication.StatusBar.SetText("Lower Limit value can't be greater to Upper limit value. @ Line # " + (i+1).ToString() , SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return false;
                    }
                    if (priority < 0)
                    {
                        oApplication.StatusBar.SetText("Define Priority, can't be negative." + (i+1).ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private Boolean ValidateUpdateRecord()
        {
            try
            {
                //checking for already use gratuity.
                if (CodeIndex.Count == 0) return false;
                string tempvalue = CodeIndex[currentRecord].ToString();
                var oDoc = (from a in dbHrPayroll.TrnsOTSlab where a.InternalID.ToString() == tempvalue select a).FirstOrDefault();
                if (oDoc == null)
                {
                    oApplication.StatusBar.SetText("validation failed document can't be found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                //int oValue = (from a in dbHrPayroll.MstEmployee where (a.GratuitySlabs != null ? a.GratuitySlabs : 0) == oDoc.InternalID select a).Count();
                //if (oValue > 0)
                //{
                //    oApplication.StatusBar.SetText("Current transaction already attached to employee can't update.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                //    return false;
                //}
                //check code change

                if (oDoc.SlabCode != txtCode.Value.Trim())
                {
                    oApplication.StatusBar.SetText("You can't change code.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }

                
                //Negate value not supported.
                grdMain.FlushToDataSource();
                for (int i = 0; i < dtMain.Rows.Count; i++)
                {
                    double lowerlimit = 0, upperlimit = 0;
                    int priority = 0;
                    lowerlimit = Convert.ToDouble(dtMain.GetValue(clLowerLimit.DataBind.Alias, i));
                    upperlimit = Convert.ToDouble(dtMain.GetValue(clUpperLimit.DataBind.Alias, i));
                    priority = Convert.ToInt32(dtMain.GetValue(clPriority.DataBind.Alias, i));
                    if ((lowerlimit < 0) || (upperlimit < 0))
                    {
                        oApplication.StatusBar.SetText("Negative value not supported. @ Line # " + (i+1).ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return false;
                    }
                    if (lowerlimit > upperlimit)
                    {
                        oApplication.StatusBar.SetText("Lower limit value can't be greater than upper limit value." + (i+1).ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return false;
                    }

                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void GetData()
        {
            try
            {
                CodeIndex.Clear();
                var oDocuments = (from a in dbHrPayroll.TrnsOTSlab select a).ToList();
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

        private void SaveRecord()
        {
            try
            {
                grdMain.FlushToDataSource();
                string currentcode = txtCode.Value.Trim();
                if (!string.IsNullOrEmpty(currentcode))
                {
                    var oDoc = (from a in dbHrPayroll.TrnsOTSlab where a.SlabCode == currentcode select a).FirstOrDefault();
                    if (oDoc != null) // document update horaha hia 
                    {
                        for (int i = 0; i < dtMain.Rows.Count; i++)
                        {
                            string otcode, id, isnew;
                            decimal lowerlimit, upperlimit;
                            int priority;
                            id = Convert.ToString(dtMain.GetValue(clID.DataBind.Alias, i));
                            isnew = Convert.ToString(dtMain.GetValue(clIsNew.DataBind.Alias, i));
                            otcode = Convert.ToString(dtMain.GetValue(clOTCode.DataBind.Alias, i));
                            lowerlimit = Convert.ToDecimal(dtMain.GetValue(clLowerLimit.DataBind.Alias, i));
                            upperlimit = Convert.ToDecimal(dtMain.GetValue(clUpperLimit.DataBind.Alias, i));
                            priority = Convert.ToInt32(dtMain.GetValue(clPriority.DataBind.Alias, i));
                            if (!string.IsNullOrEmpty(id))
                            {
                                if (Convert.ToInt32(id) != 0)
                                {
                                    TrnsOTSlabDetail oLine = null;
                                    if (isnew.ToLower() == "y")
                                    {
                                        //Yeahan per code kabhi nahi ayay ga 
                                    }
                                    else
                                    {
                                        oLine = (from a in dbHrPayroll.TrnsOTSlabDetail where a.InternalID.ToString() == id select a).FirstOrDefault();
                                        MstOverTime oOT = (from a in dbHrPayroll.MstOverTime where a.Code == otcode select a).FirstOrDefault();
                                        if (oOT != null)
                                        {
                                            oLine.OTType = oOT.ID;
                                            oLine.LowerLimit = lowerlimit;
                                            oLine.UpperLimit = upperlimit;
                                            oLine.Priority = priority;
                                            oLine.UpdateDate = DateTime.Now;
                                        }
                                    }
                                }
                                else if (Convert.ToInt32(id) == 0)
                                {
                                    TrnsOTSlabDetail oLine = null;
                                    if (isnew.ToLower() == "y")
                                    {
                                        if (!string.IsNullOrEmpty(otcode))
                                        {
                                            oLine = new TrnsOTSlabDetail();
                                            MstOverTime oOT = (from a in dbHrPayroll.MstOverTime where a.Code == otcode select a).FirstOrDefault();
                                            if (oOT != null)
                                            {
                                                oLine.OTType = oOT.ID;
                                                oLine.LowerLimit = lowerlimit;
                                                oLine.UpperLimit = upperlimit;
                                                oLine.Priority = priority;
                                                oLine.CreateDate = DateTime.Now;
                                                oLine.UpdateDate = DateTime.Now;
                                                oDoc.TrnsOTSlabDetail.Add(oLine);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        dbHrPayroll.SubmitChanges();
                        GetData();
                        InitiallizeDocument();
                        oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                    }
                    else // Add new document
                    {
                        TrnsOTSlab oNew = new TrnsOTSlab();
                        dbHrPayroll.TrnsOTSlab.InsertOnSubmit(oNew);
                        oNew.SlabCode = txtCode.Value.Trim();
                        oNew.CreateDt = DateTime.Now;
                        oNew.CreatedBy = oCompany.UserName;
                        oNew.UpdatedBy = oCompany.UserName;
                        oNew.UpdateDt = DateTime.Now;
                        for (int i = 0; i < dtMain.Rows.Count; i++)
                        {
                            string ottype, id, isnew;
                            decimal lowerlimit, upperlimit;
                            int priority;
                            id = Convert.ToString(dtMain.GetValue(clID.DataBind.Alias, i));
                            isnew = Convert.ToString(dtMain.GetValue(clIsNew.DataBind.Alias, i));
                            ottype = Convert.ToString(dtMain.GetValue(clOTCode.DataBind.Alias, i));
                            lowerlimit = Convert.ToDecimal(dtMain.GetValue(clLowerLimit.DataBind.Alias, i));
                            upperlimit = Convert.ToDecimal(dtMain.GetValue(clUpperLimit.DataBind.Alias, i));
                            priority = Convert.ToInt32(dtMain.GetValue(clPriority.DataBind.Alias, i));
                            if (!string.IsNullOrEmpty(ottype))
                            {
                                TrnsOTSlabDetail oLine = new TrnsOTSlabDetail();
                                MstOverTime oOT = (from a in dbHrPayroll.MstOverTime where a.Code == ottype select a).FirstOrDefault();
                                if (oOT != null)
                                {
                                    oLine.OTType = oOT.ID;
                                    oLine.LowerLimit = lowerlimit;
                                    oLine.UpperLimit = upperlimit;
                                    oLine.Priority = priority;
                                    oLine.CreateDate = DateTime.Now;
                                    oLine.UpdateDate = DateTime.Now;
                                    oNew.TrnsOTSlabDetail.Add(oLine);
                                }
                            }
                        }
                        dbHrPayroll.SubmitChanges();
                        GetData();
                        InitiallizeDocument();
                        oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                    }
                }
                else
                {
                    oApplication.StatusBar.SetText("Header code is empty.", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                }
            }
            catch (Exception ex)
            {
                oApplication.MessageBox(ex.Message);
            }
        }

        private void AddEmptyRow()
        {
            Int32 RowValue = 0;
            if (dtMain.Rows.Count == 0)
            {
                dtMain.Rows.Add(1);
                RowValue = dtMain.Rows.Count;
                dtMain.SetValue(clIsNew.DataBind.Alias, RowValue - 1, "Y");
                dtMain.SetValue(clID.DataBind.Alias, RowValue - 1, "0");
                dtMain.SetValue(clPicker.DataBind.Alias, RowValue - 1, strCfl);
                dtMain.SetValue(clOTCode.DataBind.Alias, RowValue - 1, "");
                dtMain.SetValue(clOTDesc.DataBind.Alias, RowValue - 1, "");
                dtMain.SetValue(clLowerLimit.DataBind.Alias, RowValue - 1, "0");
                dtMain.SetValue(clUpperLimit.DataBind.Alias, RowValue - 1, "0");
                dtMain.SetValue(clPriority.DataBind.Alias, RowValue - 1, "0");
                //grdMain.AddRow(1, RowValue + 1);
                grdMain.AddRow(1, 0);
            }
            else
            {
                if (dtMain.GetValue(clOTCode.DataBind.Alias, dtMain.Rows.Count - 1) == "")
                {
                }
                else
                {

                    dtMain.Rows.Add(1);
                    RowValue = dtMain.Rows.Count;
                    dtMain.SetValue(clIsNew.DataBind.Alias, RowValue - 1, "Y");
                    dtMain.SetValue(clID.DataBind.Alias, RowValue - 1, "0");
                    dtMain.SetValue(clPicker.DataBind.Alias, RowValue - 1, strCfl);
                    dtMain.SetValue(clOTCode.DataBind.Alias, RowValue - 1, "");
                    dtMain.SetValue(clOTDesc.DataBind.Alias, RowValue - 1, "");
                    dtMain.SetValue(clLowerLimit.DataBind.Alias, RowValue - 1, "0");
                    dtMain.SetValue(clUpperLimit.DataBind.Alias, RowValue - 1, "0");
                    dtMain.SetValue(clPriority.DataBind.Alias, RowValue - 1, "0");
                    grdMain.AddRow(1, grdMain.RowCount + 1);
                }
            }
            grdMain.LoadFromDataSource();
        }

        private void FillRecord()
        {
            try
            {
                if (CodeIndex.Count == 0) return;
                string value = CodeIndex[currentRecord].ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    var oDoc = (from a in dbHrPayroll.TrnsOTSlab where a.InternalID.ToString() == value select a).FirstOrDefault();
                    if (oDoc == null) return;
                    txtCode.Value = oDoc.SlabCode;
                    int i = 0;
                    if (oDoc.TrnsOTSlabDetail.Count > 0) dtMain.Rows.Clear();
                    foreach (var one in oDoc.TrnsOTSlabDetail)
                    {
                        dtMain.Rows.Add(1);
                        dtMain.SetValue(clID.DataBind.Alias, i, one.InternalID);
                        dtMain.SetValue(clIsNew.DataBind.Alias, i, "N");
                        dtMain.SetValue(clPicker.DataBind.Alias, i, strCfl);
                        dtMain.SetValue(clOTCode.DataBind.Alias, i, Convert.ToString(one.MstOverTime.Code));
                        dtMain.SetValue(clOTDesc.DataBind.Alias, i, Convert.ToString(one.MstOverTime.Description));
                        dtMain.SetValue(clLowerLimit.DataBind.Alias, i, Convert.ToDouble(one.LowerLimit));
                        dtMain.SetValue(clUpperLimit.DataBind.Alias, i, Convert.ToDouble(one.UpperLimit));
                        dtMain.SetValue(clPriority.DataBind.Alias, i, Convert.ToInt32(one.Priority));
                        i++;
                    }
                    AddEmptyRow();
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("fill record : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillRecord(string pvalue)
        {
            try
            {
                if (!string.IsNullOrEmpty(pvalue))
                {
                    flgValidCall = false;
                    var oDoc = (from a in dbHrPayroll.TrnsOTSlab where a.InternalID.ToString() == pvalue select a).FirstOrDefault();
                    if (oDoc == null) return;
                    txtCode.Value = oDoc.SlabCode;
                    int i = 0;
                    if (oDoc.TrnsOTSlabDetail.Count > 0) dtMain.Rows.Clear();
                    foreach (var one in oDoc.TrnsOTSlabDetail)
                    {
                        dtMain.Rows.Add(1);
                        dtMain.SetValue(clID.DataBind.Alias, i, one.InternalID);
                        dtMain.SetValue(clIsNew.DataBind.Alias, i, "N");
                        dtMain.SetValue(clPicker.DataBind.Alias, i, strCfl);
                        dtMain.SetValue(clOTCode.DataBind.Alias, i, Convert.ToString(one.MstOverTime.Code));
                        dtMain.SetValue(clOTDesc.DataBind.Alias, i, Convert.ToString(one.MstOverTime.Description));
                        dtMain.SetValue(clLowerLimit.DataBind.Alias, i, Convert.ToDouble(one.LowerLimit));
                        dtMain.SetValue(clUpperLimit.DataBind.Alias, i, Convert.ToDouble(one.UpperLimit));
                        dtMain.SetValue(clPriority.DataBind.Alias, i, Convert.ToInt32(one.Priority));
                        i++;
                    }
                    AddEmptyRow();
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                }
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("fill record : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void OpenNewSearchWindow()
        {
            try
            {
                InitiallizeDocument();
                flgValidCall = true;
                Program.EmpID = "";
                string comName = "MstSearchOTSlab";
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
                Program.objHrmsUI.logger.LogException(Program.objHrmsUI.AppVersion, ex);
            }
        }

        private void OTPicker(int rownum)
        {
            try
            {
                string strSql = sqlString.getSql("otmst", SearchKeyVal);
                picker pic = new picker(oApplication, ds.getDataTable(strSql));
                System.Data.DataTable st = pic.ShowInput("Select OT", "Select over time");
                pic = null;
                if (st.Rows.Count > 0)
                {
                    string strCode = st.Rows[0][0].ToString();
                    MstOverTime oOTDoc = (from a in dbHrPayroll.MstOverTime where a.Code == strCode select a).FirstOrDefault();
                    if (oOTDoc != null)
                    {
                        dtMain.SetValue(clOTCode.DataBind.Alias, rownum, oOTDoc.Code);
                        dtMain.SetValue(clOTDesc.DataBind.Alias, rownum, oOTDoc.Description);
                        grdMain.LoadFromDataSource();
                    }
                }
            }
            catch (Exception ex)
            {
                Program.objHrmsUI.logger.LogException(Program.objHrmsUI.AppVersion, ex);
            }
        }

        #endregion

    }
}
