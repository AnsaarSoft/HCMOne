using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using DIHRMS.Custom;
using System.Data;

namespace ACHR.Screen
{
    class frm_RefPay : HRMSBaseForm
    {
        #region Variables

        SAPbouiCOM.ComboBox cmbNonRecurring;
        SAPbouiCOM.Item IcmbNonRecurring;
        SAPbouiCOM.Matrix grdMain;
        SAPbouiCOM.DataTable dtMain;
        SAPbouiCOM.Column clMonth, clType, clValue, clStatus, clID, clIsNew;
        SAPbouiCOM.Button btnMain;


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
                case "1":
                    SaveRecord();
                    break;
                default:
                    break;
            }
        }

        public override void etBeforeClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            switch (pVal.ItemUID)
            {
                case "1":
                    if (btnMain.Caption == "Update")
                    {
                        if (!ValidateRecord())
                        {
                            BubbleEvent = false;
                        }
                    }
                    break;
                default:
                    break;
            }
        }

       
        #endregion 

        #region Functions

        private void InitiallizeForm()
        {
            try
            {
                btnMain = oForm.Items.Item("1").Specific;

                cmbNonRecurring = oForm.Items.Item("cbele").Specific;
                oForm.DataSources.UserDataSources.Add("cbele", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                cmbNonRecurring.DataBind.SetBound(true, "", "cbele");
                IcmbNonRecurring = oForm.Items.Item("cbele");

                grdMain = oForm.Items.Item("mtmain").Specific;
                dtMain = oForm.DataSources.DataTables.Item("dtmain");

                clMonth = grdMain.Columns.Item("months");
                clType = grdMain.Columns.Item("type");
                clValue = grdMain.Columns.Item("pvalue");
                clStatus = grdMain.Columns.Item("pstatus");
                clIsNew = grdMain.Columns.Item("isnew");
                clID = grdMain.Columns.Item("id");

                clIsNew.Visible = false;
                clID.Visible = false;
                grdMain.AutoResizeColumns();

                FillComboNonRecurringElements(cmbNonRecurring);
                FillComboGridType(clType);
                FillRecord();
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
                dtMain.SetValue(clIsNew.DataBind.Alias, RowValue - 1, "Y");
                dtMain.SetValue(clID.DataBind.Alias, RowValue - 1, "0");
                dtMain.SetValue(clMonth.DataBind.Alias, RowValue - 1, 0);
                dtMain.SetValue(clType.DataBind.Alias, RowValue - 1, "-1");
                dtMain.SetValue(clValue.DataBind.Alias, RowValue - 1, 0);
                dtMain.SetValue(clStatus.DataBind.Alias, RowValue - 1, "Y");
                grdMain.AddRow(1, RowValue + 1);
            }
            else
            {
                if (dtMain.GetValue(clMonth.DataBind.Alias, dtMain.Rows.Count - 1) == 0)
                {
                }
                else
                {

                    dtMain.Rows.Add(1);
                    RowValue = dtMain.Rows.Count;
                    dtMain.SetValue(clIsNew.DataBind.Alias, RowValue - 1, "Y");
                    dtMain.SetValue(clID.DataBind.Alias, RowValue - 1, "0");
                    dtMain.SetValue(clMonth.DataBind.Alias, RowValue - 1, 0);
                    dtMain.SetValue(clType.DataBind.Alias, RowValue - 1, "-1");
                    dtMain.SetValue(clValue.DataBind.Alias, RowValue - 1, 0);
                    dtMain.SetValue(clStatus.DataBind.Alias, RowValue - 1, "Y");
                    grdMain.AddRow(1, grdMain.RowCount + 1);
                }
            }
            grdMain.LoadFromDataSource();
        }

        private void FillComboNonRecurringElements(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                var oCollection = (from Head in dbHrPayroll.MstElements
                                   join Detail in dbHrPayroll.MstElementEarning on Head.Id equals Detail.ElementID
                                   where Head.Type == "Non-Rec" && Head.ElmtType == "Ear" && Detail.ValueType == "FIX"
                                   select new { Id = Head.Id, ElementName = Head.ElementName }).ToList();
                if (oCollection.Count > 0)
                {
                    pCombo.ValidValues.Add("-1", "Select Element");
                    foreach (var One in oCollection)
                    {
                        pCombo.ValidValues.Add(Convert.ToString(One.Id), Convert.ToString(One.ElementName));
                    }
                    pCombo.Select("-1", SAPbouiCOM.BoSearchKey.psk_ByValue);
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("FillComboNonRecurringElements : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillComboGridType(SAPbouiCOM.Column pCombo)
        {
            try
            {
                var oCollection = (from a in dbHrPayroll.MstLOVE where a.Type == "LevDed_Type" select a).ToList();
                if (oCollection.Count > 0)
                {
                    pCombo.ValidValues.Add("-1", "Select Element");
                    foreach (var One in oCollection)
                    {
                        pCombo.ValidValues.Add(Convert.ToString(One.Id), Convert.ToString(One.Value));
                    }
                    
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("FillComboGridType : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void SaveRecord()
        {
            try
            {
                string elementvalue = cmbNonRecurring.Value.Trim();
                var oElement = (from a in dbHrPayroll.MstElements where a.Id.ToString() == elementvalue select a).FirstOrDefault();
                grdMain.FlushToDataSource();
                for (int i = 0; i < dtMain.Rows.Count; i++)
                {
                    string month = Convert.ToString(dtMain.GetValue(clMonth.DataBind.Alias, i));
                    string ptype = Convert.ToString(dtMain.GetValue(clType.DataBind.Alias, i));
                    string pvalue = Convert.ToString(dtMain.GetValue(clValue.DataBind.Alias, i));
                    string pstatus = Convert.ToString(dtMain.GetValue(clStatus.DataBind.Alias, i));
                    string gridid = Convert.ToString(dtMain.GetValue(clID.DataBind.Alias, i));
                    string isnew = Convert.ToString(dtMain.GetValue(clIsNew.DataBind.Alias, i));
                    if (isnew.ToLower() == "y")
                    {
                        if (!string.IsNullOrEmpty(month))
                        {
                            MstReferralSchemes oDoc = new MstReferralSchemes();
                            oDoc.Months = Convert.ToInt32(month);
                            oDoc.PType = Convert.ToString(ptype);
                            oDoc.PValue = Convert.ToDecimal(pvalue);
                            oDoc.MstElements = oElement;
                            if (pstatus.ToLower() == "y")
                            {
                                oDoc.FlgActive = true;
                            }
                            else
                            {
                                oDoc.FlgActive = false;
                            }
                            oDoc.CreateDate = DateTime.Now;
                            oDoc.UpdateDate = DateTime.Now;
                            oDoc.CreatedBy = oCompany.UserName;
                            oDoc.UpdatedBy = oCompany.UserName;
                            dbHrPayroll.MstReferralSchemes.InsertOnSubmit(oDoc);
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(month))
                        {
                            MstReferralSchemes oDoc = (from a in dbHrPayroll.MstReferralSchemes where a.InternalID.ToString() == gridid select a).FirstOrDefault();
                            oDoc.Months = Convert.ToInt32(month);
                            oDoc.PType = Convert.ToString(ptype);
                            oDoc.PValue = Convert.ToDecimal(pvalue);
                            oDoc.MstElements = oElement;
                            if (pstatus.ToLower() == "y")
                            {
                                oDoc.FlgActive = true;
                            }
                            else
                            {
                                oDoc.FlgActive = false;
                            }
                            oDoc.UpdateDate = DateTime.Now;
                            oDoc.UpdatedBy = oCompany.UserName;
                        }
                    }
                }
                dbHrPayroll.SubmitChanges();
                FillRecord();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("SaveRecord : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillRecord()
        {
            oForm.Freeze(true);
            try
            {
                dtMain.Rows.Clear();
                string elementvalue = "";
                var oCollection = (from a in dbHrPayroll.MstReferralSchemes select a).ToList();
                if (oCollection.Count > 0)
                {
                    for (int i = 0; i < oCollection.Count; i++)
                    {
                        dtMain.Rows.Add();
                        if (oCollection[i].MstElements != null)
                        {
                            elementvalue = Convert.ToString(oCollection[i].MstElements.Id);
                        }
                        else
                        {
                            elementvalue = "";
                        }
                        dtMain.SetValue(clID.DataBind.Alias, i, oCollection[i].InternalID);
                        dtMain.SetValue(clIsNew.DataBind.Alias, i, "N");
                        dtMain.SetValue(clMonth.DataBind.Alias, i, oCollection[i].Months);
                        dtMain.SetValue(clType.DataBind.Alias, i, oCollection[i].PType);
                        dtMain.SetValue(clValue.DataBind.Alias, i, Convert.ToDouble(oCollection[i].PValue));
                        dtMain.SetValue(clStatus.DataBind.Alias, i, oCollection[i].FlgActive != null ? (oCollection[i].FlgActive == true ? "Y" : "N") : "N");
                    }
                    cmbNonRecurring.Select(elementvalue, SAPbouiCOM.BoSearchKey.psk_ByValue);
                }
                AddEmptyRow();
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("FillRecord : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            oForm.Freeze(false);
        }

        private Boolean ValidateRecord()
        {
            try
            {

                //Negate value not supported.
                grdMain.FlushToDataSource();
                for (int i = 0; i < dtMain.Rows.Count; i++)
                {
                    double refvalue = 0;
                    refvalue = Convert.ToDouble(dtMain.GetValue(clValue.DataBind.Alias, i));
                    if (refvalue < 0)
                    {
                        oApplication.StatusBar.SetText("Negative value not supported.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
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

        #endregion 
    }
}
