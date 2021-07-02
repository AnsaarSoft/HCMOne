using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using DIHRMS.Custom;
using System.Data;
using System.Data.SqlClient;
using SAPbobsCOM;
using SAPbouiCOM;
using System.Collections;
using System.IO;
using UFFU;

namespace ACHR.Screen
{
    class frm_PRElem : HRMSBaseForm
    {

        #region Varialbes

        Matrix grdMain;
        SAPbouiCOM.DataTable dtMain;
        Column clCode, clDesc, clType, clValueType, clValue, clFunction, clElement, clStatus, clId, clSerial;
        Button btnMain, btnCancel;

        bool flgDocLoad = false;

        #endregion

        #region B1 Events

        public override void CreateForm(Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.Freeze(true);
            try
            {
                InitiallizeForm();
            }
            catch (Exception ex)
            {
                logger(ex);
            }
            oForm.Freeze(false);
            flgDocLoad = true;
        }

        public override void etAfterLostFocus(ref ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterLostFocus(ref pVal, ref BubbleEvent);
            if (!flgDocLoad) return;
            oForm.Freeze(true);
            try
            {
                if (pVal.ItemUID == grdMain.Item.UniqueID && pVal.ColUID == clStatus.UniqueID)
                {
                    grdMain.FlushToDataSource();
                    AddEmptyLine();
                }
            }
            catch (Exception ex)
            {
                logger(ex);
            }
            oForm.Freeze(false);
        }

        public override void etBeforeClick(ref ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etBeforeClick(ref pVal, ref BubbleEvent);
            if (!flgDocLoad) return;
            try
            {
                if (pVal.ItemUID == btnMain.Item.UniqueID)
                {
                    if (!ValidateRecord())
                    {
                        BubbleEvent = false;
                    }
                }
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        public override void etAfterClick(ref ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            if (!flgDocLoad) return;
            try
            {
                if (pVal.ItemUID == btnMain.Item.UniqueID)
                {
                    if (SubmitRecord())
                    {
                        FillRecords();
                    }

                }
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        #endregion

        #region Functions

        private void InitiallizeForm()
        {
            try
            {
                grdMain = oForm.Items.Item("mtmain").Specific;
                dtMain = oForm.DataSources.DataTables.Item("dtmain");
                clCode = grdMain.Columns.Item("clCode");
                clDesc = grdMain.Columns.Item("cldesc");
                clType = grdMain.Columns.Item("cltype");
                clValueType = grdMain.Columns.Item("clvaltype");
                clValue = grdMain.Columns.Item("clVal");
                clFunction = grdMain.Columns.Item("clfunc");
                clElement = grdMain.Columns.Item("clelem");
                clStatus = grdMain.Columns.Item("clStatus");
                clId = grdMain.Columns.Item("clId");
                clId.Visible = false;
                clSerial = grdMain.Columns.Item("clSerial");

                btnMain = oForm.Items.Item("1").Specific;
                btnCancel = oForm.Items.Item("2").Specific;

                FillElementType();
                FillValueType();
                FillValueFunctions();
                FillElements();
                FillRecords();
                AddEmptyLine();
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private void FillElementType()
        {
            try
            {
                var oCollection = (from a in dbHrPayroll.MstLOVE
                                   where a.Type == "PRElemType"
                                   select a).ToList();
                clType.ValidValues.Add("0", "Select Type");
                foreach (var One in oCollection)
                {
                    clType.ValidValues.Add(One.Code, One.Value);
                }
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private void FillValueType()
        {
            try
            {
                var oCollection = (from a in dbHrPayroll.MstLOVE
                                   where a.Type == "PRElemVType"
                                   select a).ToList();
                clValueType.ValidValues.Add("0", "Select Value Type");
                foreach (var One in oCollection)
                {
                    clValueType.ValidValues.Add(One.Code, One.Value);
                }
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private void FillValueFunctions()
        {
            try
            {
                var oCollection = (from a in dbHrPayroll.MstLOVE
                                   where a.Type == "PRFunction"
                                   select a).ToList();
                clFunction.ValidValues.Add("0", "Select Value.");
                foreach (var One in oCollection)
                {
                    clFunction.ValidValues.Add(One.Code, One.Value);
                }
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private void FillElements()
        {
            try
            {
                var oCollection = (from a in dbHrPayroll.MstElements
                                   where a.Type == "Non-Rec"
                                   select a).ToList();
                clElement.ValidValues.Add("0", "Select Value.");
                foreach (var One in oCollection)
                {
                    if (One.ElmtType == "Ear")
                    {
                        if (One.MstElementEarning[0].ValueType.Trim().ToUpper() == "FIX")
                        {
                            clElement.ValidValues.Add(One.Id.ToString(), One.Description);
                        }
                    }
                    else if (One.ElmtType == "Ded")
                    {
                        if (One.MstElementDeduction[0].ValueType.Trim().ToUpper() == "FIX")
                        {
                            clElement.ValidValues.Add(One.Id.ToString(), One.Description);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private void AddEmptyLine()
        {
            try
            {
                Int32 RowValue = 0;
                if (dtMain.Rows.Count == 0)
                {
                    dtMain.Rows.Add(1);
                    RowValue = dtMain.Rows.Count;
                    dtMain.SetValue(clId.DataBind.Alias, RowValue - 1, 0);
                    dtMain.SetValue(clSerial.DataBind.Alias, RowValue - 1, RowValue);
                    dtMain.SetValue(clCode.DataBind.Alias, RowValue - 1, "");
                    dtMain.SetValue(clDesc.DataBind.Alias, RowValue - 1, "");
                    dtMain.SetValue(clType.DataBind.Alias, RowValue - 1, "0");
                    dtMain.SetValue(clValueType.DataBind.Alias, RowValue - 1, "0");
                    dtMain.SetValue(clValue.DataBind.Alias, RowValue - 1, 0d);
                    dtMain.SetValue(clFunction.DataBind.Alias, RowValue - 1, "0");
                    dtMain.SetValue(clElement.DataBind.Alias, RowValue - 1, "0");
                    dtMain.SetValue(clStatus.DataBind.Alias, RowValue - 1, "Y");
                    grdMain.AddRow(1, 0);
                }
                else
                {
                    if (dtMain.GetValue(clCode.DataBind.Alias, dtMain.Rows.Count - 1) == "")
                    {
                    }
                    else
                    {

                        dtMain.Rows.Add(1);
                        RowValue = dtMain.Rows.Count;
                        dtMain.SetValue(clId.DataBind.Alias, RowValue - 1, 0);
                        dtMain.SetValue(clSerial.DataBind.Alias, RowValue - 1, RowValue);
                        dtMain.SetValue(clCode.DataBind.Alias, RowValue - 1, "");
                        dtMain.SetValue(clDesc.DataBind.Alias, RowValue - 1, "");
                        dtMain.SetValue(clType.DataBind.Alias, RowValue - 1, "0");
                        dtMain.SetValue(clValueType.DataBind.Alias, RowValue - 1, "0");
                        dtMain.SetValue(clValue.DataBind.Alias, RowValue - 1, 0d);
                        dtMain.SetValue(clFunction.DataBind.Alias, RowValue - 1, "0");
                        dtMain.SetValue(clElement.DataBind.Alias, RowValue - 1, "0");
                        dtMain.SetValue(clStatus.DataBind.Alias, RowValue - 1, "Y");
                        grdMain.AddRow(1, grdMain.RowCount + 1);
                    }
                }
                grdMain.LoadFromDataSource();
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private bool SubmitRecord()
        {
            try
            {
                string code, desc, elementtype, valuetype, functionvalue, status, elementcode;
                int elementid = 0, lineid;
                double value;
                bool flgstatus = false;
                grdMain.FlushToDataSource();
                for (int i = 0; i < dtMain.Rows.Count; i++)
                {
                    code = dtMain.GetValue(clCode.DataBind.Alias, i);
                    desc = dtMain.GetValue(clDesc.DataBind.Alias, i);
                    elementtype = dtMain.GetValue(clType.DataBind.Alias, i);
                    valuetype = dtMain.GetValue(clValueType.DataBind.Alias, i);
                    value = Convert.ToDouble(dtMain.GetValue(clValue.DataBind.Alias, i));
                    functionvalue = dtMain.GetValue(clFunction.DataBind.Alias, i);
                    status = dtMain.GetValue(clStatus.DataBind.Alias, i);
                    elementcode = dtMain.GetValue(clElement.DataBind.Alias, i);
                    if (!string.IsNullOrEmpty(elementcode))
                    {
                        elementid = (from a in dbHrPayroll.MstElements
                                     where a.Id.ToString() == elementcode
                                     select a.Id).FirstOrDefault();
                    }
                    lineid = dtMain.GetValue(clId.DataBind.Alias, i);
                    if (!string.IsNullOrEmpty(status))
                    {
                        if (status.Trim().ToUpper() == "Y")
                        {
                            flgstatus = true;
                        }
                        else
                        {
                            flgstatus = false;
                        }
                    }
                    if (lineid == 0)
                    {
                        if (string.IsNullOrEmpty(code)) continue;
                        MstElementsPerRate oDoc = new MstElementsPerRate();
                        dbHrPayroll.MstElementsPerRate.InsertOnSubmit(oDoc);
                        oDoc.Code = code;
                        oDoc.Description = desc;
                        oDoc.ElemType = elementtype;
                        oDoc.ValueType = valuetype;
                        oDoc.ElemValue = (decimal)value;
                        oDoc.ElemFunction = functionvalue;
                        oDoc.PayThrough = elementid;
                        oDoc.FlgActive = flgstatus;
                        oDoc.CreatedBy = oCompany.UserName;
                        oDoc.UpdatedBy = oCompany.UserName;
                        oDoc.CreateDate = DateTime.Now;
                        oDoc.UpdateDate = DateTime.Now;
                    }
                    else
                    {
                        MstElementsPerRate oDoc = (from a in dbHrPayroll.MstElementsPerRate
                                                   where a.ID == lineid
                                                   select a).FirstOrDefault();
                        if (oDoc == null) continue;
                        oDoc.Description = desc;
                        oDoc.ElemType = elementtype;
                        oDoc.ValueType = valuetype;
                        oDoc.ElemValue = (decimal)value;
                        oDoc.ElemFunction = functionvalue;
                        oDoc.PayThrough = elementid;
                        oDoc.FlgActive = flgstatus;
                        oDoc.UpdatedBy = oCompany.UserName;
                        oDoc.UpdateDate = DateTime.Now;
                    }
                }
                dbHrPayroll.SubmitChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger(ex);
                return false;
            }
        }

        private bool ValidateRecord()
        {
            try
            {

                return true;
            }
            catch (Exception ex)
            {
                logger(ex);
                return false;
            }
        }

        private void FillRecords()
        {
            try
            {
                dtMain.Rows.Clear();
                var oCollection = (from a in dbHrPayroll.MstElementsPerRate
                                   where a.FlgActive == true
                                   select a).ToList();
                int i = 0;
                foreach (var One in oCollection)
                {
                    dtMain.Rows.Add(1);
                    dtMain.SetValue(clId.DataBind.Alias, i, One.ID);
                    dtMain.SetValue(clSerial.DataBind.Alias, i, i + 1);
                    dtMain.SetValue(clCode.DataBind.Alias, i, One.Code);
                    dtMain.SetValue(clDesc.DataBind.Alias, i, One.Description);
                    dtMain.SetValue(clType.DataBind.Alias, i, One.ElemType);
                    dtMain.SetValue(clValueType.DataBind.Alias, i, One.ValueType);
                    dtMain.SetValue(clValue.DataBind.Alias, i, Convert.ToDouble(One.ElemValue));
                    dtMain.SetValue(clFunction.DataBind.Alias, i, One.ElemFunction);
                    dtMain.SetValue(clElement.DataBind.Alias, i, One.PayThrough);
                    dtMain.SetValue(clStatus.DataBind.Alias, i, "Y");
                    i++;
                }
                grdMain.LoadFromDataSource();
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        #endregion

    }
}
