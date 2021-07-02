using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using DIHRMS.Custom;
using System.Data;

namespace ACHR.Screen
{
    class frm_GratuitySlabs:HRMSBaseForm
    {
        #region Variables

        SAPbouiCOM.EditText txtCode, txtBasedOnValue, txtDays;
        SAPbouiCOM.ComboBox cmbBasedOn;
        SAPbouiCOM.Item itxtCode, itxtBasedOnValue, itxtDays;
        SAPbouiCOM.DataTable dtMain;
        SAPbouiCOM.Matrix grdMain;
        SAPbouiCOM.Column clID, clIsNew, clDescription, clFromYear, clToYear, clDaysCount;
        SAPbouiCOM.Button btnMain, btnCancel;
        SAPbouiCOM.CheckBox chkWOPLeaves, chkAbsoluteYear, chkPerYear;


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
                    case "2":
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
                    default :
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
            if (pVal.ItemUID == "mtMain" && pVal.ColUID == "cldc")
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


                txtBasedOnValue = oForm.Items.Item("txbvalue").Specific;
                oForm.DataSources.UserDataSources.Add("txbvalue", SAPbouiCOM.BoDataType.dt_QUANTITY);
                itxtBasedOnValue = oForm.Items.Item("txbvalue");
                txtBasedOnValue.DataBind.SetBound(true, "", "txbvalue");
                txtBasedOnValue.TabOrder = 3;

                txtDays = oForm.Items.Item("txdays").Specific;
                oForm.DataSources.UserDataSources.Add("txdays", SAPbouiCOM.BoDataType.dt_QUANTITY);
                itxtDays = oForm.Items.Item("txdays");
                txtDays.DataBind.SetBound(true, "", "txdays");
                txtDays.TabOrder = 4;
                
                cmbBasedOn = oForm.Items.Item("cbbased").Specific;
                oForm.DataSources.UserDataSources.Add("cbbased", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cmbBasedOn.DataBind.SetBound(true, "", "cbbased");
                cmbBasedOn.TabOrder = 2;

                chkWOPLeaves = oForm.Items.Item("chwop").Specific;
                oForm.DataSources.UserDataSources.Add("chwop", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                chkWOPLeaves.DataBind.SetBound(true, "", "chwop");

                chkAbsoluteYear = oForm.Items.Item("chayear").Specific;
                oForm.DataSources.UserDataSources.Add("chayear", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                chkAbsoluteYear.DataBind.SetBound(true, "", "chayear");

                chkPerYear = oForm.Items.Item("chperyear").Specific;
                oForm.DataSources.UserDataSources.Add("chperyear", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                chkPerYear.DataBind.SetBound(true, "", "chperyear");

                grdMain = oForm.Items.Item("mtMain").Specific;
                dtMain = oForm.DataSources.DataTables.Item("dtMain");
                clDescription = grdMain.Columns.Item("cldesc");
                clID = grdMain.Columns.Item("clid");
                clID.Visible = false;
                clIsNew = grdMain.Columns.Item("clisnew");
                clIsNew.Visible = false;
                clFromYear = grdMain.Columns.Item("clfy");
                clToYear = grdMain.Columns.Item("clty");
                clDaysCount = grdMain.Columns.Item("cldc");

                btnMain = oForm.Items.Item("1").Specific;
                btnCancel = oForm.Items.Item("2").Specific;

                FillBasedOnCombo(cmbBasedOn);

                InitiallizeDocument();
                GetData();
                
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
                cmbBasedOn.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                txtBasedOnValue.Value = "";
                txtDays.Value = "";
                chkWOPLeaves.Checked = true;
                chkAbsoluteYear.Checked = true;
                chkPerYear.Checked = true;
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

        private void FillBasedOnCombo(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                var oCollection = (from a in dbHrPayroll.MstLOVE where a.Type == "gratBasedOn" select a).ToList();
                pCombo.ValidValues.Add("-1", "");
                foreach (var One in oCollection)
                {
                    pCombo.ValidValues.Add(Convert.ToString(One.Code), Convert.ToString(One.Value));
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("FillBasedOnCombo : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }

        }

        private Boolean ValidateAddRecord()
        {
            try
            {
                //checking for duplicate code

                string tempvalue = txtCode.Value.Trim();
                var oDoc = (from a in dbHrPayroll.TrnsGratuitySlabs where a.SlabCode.ToLower() == tempvalue.ToLower() select a).FirstOrDefault();
                if (oDoc != null)
                {
                    oApplication.StatusBar.SetText("Define code already in use.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }

                //checking for empty based on 
                if (cmbBasedOn.Value.Trim() == "-1")
                {
                    oApplication.StatusBar.SetText("Select Based On value.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                string calvalue = txtBasedOnValue.Value.Trim();
                if (!string.IsNullOrEmpty(calvalue))
                {
                    decimal check = Convert.ToDecimal(calvalue);
                    if (check <= 0)
                    {
                        oApplication.StatusBar.SetText("Value can't be negative.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return false;
                    }
                }
                string caldays = txtDays.Value.Trim();
                if (!string.IsNullOrEmpty(caldays))
                {
                    decimal check = Convert.ToDecimal(caldays);
                    if (check < 0)
                    {
                        oApplication.StatusBar.SetText("Calculated days can't be negative.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return false;
                    }
                }
                //Negate value not supported.
                grdMain.FlushToDataSource();
                for (int i = 0; i < dtMain.Rows.Count; i++)
                {
                    double fromvalue = 0, tovalue = 0;
                    fromvalue = Convert.ToDouble(dtMain.GetValue(clFromYear.DataBind.Alias, i));
                    tovalue = Convert.ToDouble(dtMain.GetValue(clToYear.DataBind.Alias, i));
                    if ((fromvalue < 0) || (tovalue < 0))
                    {
                        oApplication.StatusBar.SetText("Negative not supported.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return false;
                    }
                    if (fromvalue > tovalue)
                    {
                        oApplication.StatusBar.SetText("From value can't be greater to value.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
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
                var oDoc = (from a in dbHrPayroll.TrnsGratuitySlabs where a.InternalID.ToString() == tempvalue select a).FirstOrDefault();
                if (oDoc == null)
                {
                    oApplication.StatusBar.SetText("validation failed document can't be found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                int oValue = (from a in dbHrPayroll.MstEmployee where (a.GratuitySlabs != null ? a.GratuitySlabs : 0) == oDoc.InternalID select a).Count();
                if (oValue > 0)
                {
                    oApplication.StatusBar.SetText("Current transaction already attached to employee can't update.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                //check code change
                
                if (oDoc.SlabCode != txtCode.Value.Trim())
                {
                    oApplication.StatusBar.SetText("You can't change code.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }

                //check combo value
                if (cmbBasedOn.Value.Trim() == "-1")
                {
                    oApplication.StatusBar.SetText("Select based on value its mandatory.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                string calvalue = txtBasedOnValue.Value.Trim();
                if (!string.IsNullOrEmpty(calvalue))
                {
                    decimal check = Convert.ToDecimal(calvalue);
                    if (check <= 0)
                    {
                        oApplication.StatusBar.SetText("Value can't be negative.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return false;
                    }
                }
                string caldays = txtDays.Value.Trim();
                if (!string.IsNullOrEmpty(caldays))
                {
                    decimal check = Convert.ToDecimal(caldays);
                    if (check < 0)
                    {
                        oApplication.StatusBar.SetText("Calculated days can't be negative.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return false;
                    }
                }
                //Negate value not supported.
                grdMain.FlushToDataSource();
                for (int i = 0; i < dtMain.Rows.Count; i++)
                {
                    double fromvalue = 0, tovalue = 0;
                    fromvalue = Convert.ToDouble(dtMain.GetValue(clFromYear.DataBind.Alias, i));
                    tovalue = Convert.ToDouble(dtMain.GetValue(clToYear.DataBind.Alias, i));
                    if ((fromvalue < 0) || (tovalue < 0))
                    {
                        oApplication.StatusBar.SetText("Negative not supported.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return false;
                    }
                    if (fromvalue > tovalue)
                    {
                        oApplication.StatusBar.SetText("From value can't be greater to value.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void GetData()
        {
            try
            {
                CodeIndex.Clear();
                var oDocuments = (from a in dbHrPayroll.TrnsGratuitySlabs select a).ToList();
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
                string basedon = cmbBasedOn.Value.Trim();
                string basedonvalue = txtBasedOnValue.Value.Trim();
                string calculateddays = txtDays.Value.Trim();
                Boolean wopleaves = chkWOPLeaves.Checked;
                Boolean absoluteyear = chkAbsoluteYear.Checked;
                Boolean flgPerYear = chkPerYear.Checked;
                if (!string.IsNullOrEmpty(currentcode))
                {
                    var oDoc = (from a in dbHrPayroll.TrnsGratuitySlabs where a.SlabCode == currentcode select a).FirstOrDefault();
                    if (oDoc != null) // document update horaha hia 
                    {
                        oDoc.BasedOn = basedon;
                        oDoc.BasedOnValue = Convert.ToDecimal(basedonvalue);
                        oDoc.CalculatedDays = Convert.ToDecimal(calculateddays);
                        oDoc.FlgWOPLeaves = wopleaves;
                        oDoc.FlgAbsoluteYears = absoluteyear;
                        oDoc.FlgPerYear = flgPerYear;
                        for (int i = 0; i < dtMain.Rows.Count; i++)
                        {
                            string desc, id, isnew;
                            decimal fromyear, toyear, dayscount;
                            id = Convert.ToString(dtMain.GetValue(clID.DataBind.Alias, i));
                            isnew = Convert.ToString(dtMain.GetValue(clIsNew.DataBind.Alias, i));
                            desc = Convert.ToString(dtMain.GetValue(clDescription.DataBind.Alias, i));
                            fromyear = Convert.ToDecimal(dtMain.GetValue(clFromYear.DataBind.Alias, i));
                            toyear = Convert.ToDecimal(dtMain.GetValue(clToYear.DataBind.Alias, i));
                            dayscount = Convert.ToDecimal(dtMain.GetValue(clDaysCount.DataBind.Alias, i));
                            if (!string.IsNullOrEmpty(id))
                            {
                                if (Convert.ToInt32(id) != 0)
                                {
                                    TrnsGratuitySlabsDetail oLine = null;
                                    if (isnew.ToLower() == "y")
                                    {
                                        //Yeahan per code kabhi nahi ayay ga 
                                    }
                                    else
                                    {
                                        oLine = (from a in dbHrPayroll.TrnsGratuitySlabsDetail where a.InternalID.ToString() == id select a).FirstOrDefault();
                                        oLine.Description = desc;
                                        oLine.FromPoints = fromyear;
                                        oLine.ToPoints = toyear;
                                        oLine.DaysCount = dayscount;
                                        oLine.UpdateDate = DateTime.Now;
                                       
                                    }
                                }
                                else if (Convert.ToInt32(id) == 0)
                                {
                                    TrnsGratuitySlabsDetail oLine = null;
                                    if (isnew.ToLower() == "y")
                                    {
                                        if (!string.IsNullOrEmpty(desc))
                                        {
                                            oLine = new TrnsGratuitySlabsDetail();
                                            oLine.Description = desc;
                                            oLine.FromPoints = fromyear;
                                            oLine.ToPoints = toyear;
                                            oLine.DaysCount = dayscount;
                                            oLine.CreateDate = DateTime.Now;
                                            oLine.UpdateDate = DateTime.Now;
                                            oDoc.TrnsGratuitySlabsDetail.Add(oLine);
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
                        TrnsGratuitySlabs oNew = new TrnsGratuitySlabs();
                        dbHrPayroll.TrnsGratuitySlabs.InsertOnSubmit(oNew);
                        oNew.SlabCode = txtCode.Value.Trim();
                        oNew.BasedOn = cmbBasedOn.Value.Trim();
                        oNew.BasedOnValue = Convert.ToDecimal(txtBasedOnValue.Value.Trim());
                        oNew.CalculatedDays = Convert.ToDecimal(txtDays.Value.Trim());
                        oNew.FlgWOPLeaves = chkWOPLeaves.Checked;
                        oNew.CreateDate = DateTime.Now;
                       // oNew.UpdateDate = DateTime.Now;
                        oNew.CreatedBy = oCompany.UserName;
                        //oNew.UpdatedBy = oCompany.UserName;
                        for (int i = 0; i < dtMain.Rows.Count; i++)
                        {
                            string desc, id, isnew;
                            decimal fromyear, toyear, dayscount;
                            id = Convert.ToString(dtMain.GetValue(clID.DataBind.Alias, i));
                            isnew = Convert.ToString(dtMain.GetValue(clIsNew.DataBind.Alias, i));
                            desc = Convert.ToString(dtMain.GetValue(clDescription.DataBind.Alias, i));
                            fromyear = Convert.ToDecimal(dtMain.GetValue(clFromYear.DataBind.Alias, i));
                            toyear = Convert.ToDecimal(dtMain.GetValue(clToYear.DataBind.Alias, i));
                            dayscount = Convert.ToDecimal(dtMain.GetValue(clDaysCount.DataBind.Alias, i));
                            if (!string.IsNullOrEmpty(desc))
                            {
                                TrnsGratuitySlabsDetail oLine = new TrnsGratuitySlabsDetail();
                                oLine.Description = desc;
                                oLine.FromPoints = fromyear;
                                oLine.ToPoints = toyear;
                                oLine.DaysCount = dayscount;
                                oLine.CreateDate = DateTime.Now;
                                oLine.UpdateDate = DateTime.Now;
                                oNew.TrnsGratuitySlabsDetail.Add(oLine);
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
                dtMain.SetValue(clDescription.DataBind.Alias, RowValue - 1, "");
                dtMain.SetValue(clFromYear.DataBind.Alias, RowValue - 1, "0");
                dtMain.SetValue(clToYear.DataBind.Alias, RowValue - 1, "0");
                dtMain.SetValue(clDaysCount.DataBind.Alias, RowValue - 1, "0");
                //grdMain.AddRow(1, RowValue + 1);
                grdMain.AddRow(1, 0);
            }
            else
            {
                if (dtMain.GetValue(clDescription.DataBind.Alias, dtMain.Rows.Count - 1) == "")
                {
                }
                else
                {

                    dtMain.Rows.Add(1);
                    RowValue = dtMain.Rows.Count;
                    dtMain.SetValue(clIsNew.DataBind.Alias, RowValue - 1, "Y");
                    dtMain.SetValue(clID.DataBind.Alias, RowValue - 1, "0");
                    dtMain.SetValue(clDescription.DataBind.Alias, RowValue - 1, "");
                    dtMain.SetValue(clFromYear.DataBind.Alias, RowValue - 1, "0");
                    dtMain.SetValue(clToYear.DataBind.Alias, RowValue - 1, "0");
                    dtMain.SetValue(clDaysCount.DataBind.Alias, RowValue - 1, "0");                    
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
                    var oDoc = (from a in dbHrPayroll.TrnsGratuitySlabs where a.InternalID.ToString() == value select a).FirstOrDefault();
                    if (oDoc == null) return;
                    txtCode.Value = oDoc.SlabCode;
                    cmbBasedOn.Select(oDoc.BasedOn != null ? Convert.ToString(oDoc.BasedOn) : "0", SAPbouiCOM.BoSearchKey.psk_ByValue);
                    txtBasedOnValue.Value = Convert.ToString(oDoc.BasedOnValue);
                    txtDays.Value = Convert.ToString(oDoc.CalculatedDays);
                    chkWOPLeaves.Checked = oDoc.FlgWOPLeaves != null ? Convert.ToBoolean(oDoc.FlgWOPLeaves) : false;
                    chkAbsoluteYear.Checked = oDoc.FlgAbsoluteYears != null ? Convert.ToBoolean(oDoc.FlgAbsoluteYears) : false;
                    chkPerYear.Checked = oDoc.FlgPerYear != null ? Convert.ToBoolean(oDoc.FlgPerYear) : false;
                    int i = 0;
                    if (oDoc.TrnsGratuitySlabsDetail.Count > 0) dtMain.Rows.Clear();
                    foreach (var one in oDoc.TrnsGratuitySlabsDetail)
                    {
                        dtMain.Rows.Add(1);
                        dtMain.SetValue(clID.DataBind.Alias, i, one.InternalID);
                        dtMain.SetValue(clIsNew.DataBind.Alias, i, "N");
                        dtMain.SetValue(clDescription.DataBind.Alias, i, Convert.ToString(one.Description));
                        dtMain.SetValue(clFromYear.DataBind.Alias, i, Convert.ToDouble(one.FromPoints));
                        dtMain.SetValue(clToYear.DataBind.Alias, i, Convert.ToDouble(one.ToPoints));
                        dtMain.SetValue(clDaysCount.DataBind.Alias, i, Convert.ToDouble(one.DaysCount));
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
                    var oDoc = (from a in dbHrPayroll.TrnsGratuitySlabs where a.InternalID.ToString() == pvalue select a).FirstOrDefault();
                    if (oDoc == null) return;
                    txtCode.Value = oDoc.SlabCode;
                    cmbBasedOn.Select(oDoc.BasedOn != null ? Convert.ToString(oDoc.BasedOn) : "0", SAPbouiCOM.BoSearchKey.psk_ByValue);
                    txtBasedOnValue.Value = Convert.ToString(oDoc.BasedOnValue);
                    txtDays.Value = Convert.ToString(oDoc.CalculatedDays);
                    chkWOPLeaves.Checked = oDoc.FlgWOPLeaves != null ? Convert.ToBoolean(oDoc.FlgWOPLeaves) : false;
                    int i = 0;
                    if (oDoc.TrnsGratuitySlabsDetail.Count > 0) dtMain.Rows.Clear();
                    foreach (var one in oDoc.TrnsGratuitySlabsDetail)
                    {
                        dtMain.Rows.Add(1);
                        dtMain.SetValue(clID.DataBind.Alias, i, one.InternalID);
                        dtMain.SetValue(clIsNew.DataBind.Alias, i, "N");
                        dtMain.SetValue(clDescription.DataBind.Alias, i, Convert.ToString(one.Description));
                        dtMain.SetValue(clFromYear.DataBind.Alias, i, Convert.ToDouble(one.FromPoints));
                        dtMain.SetValue(clToYear.DataBind.Alias, i, Convert.ToDouble(one.ToPoints));
                        dtMain.SetValue(clDaysCount.DataBind.Alias, i, Convert.ToDouble(one.DaysCount));
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
                string comName = "MstSearchGrautity";
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
