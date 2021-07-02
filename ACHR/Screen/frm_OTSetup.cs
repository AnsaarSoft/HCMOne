
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_OTSetup:HRMSBaseForm
    {
        /* Form Items Objects */
        #region Variables
        SAPbouiCOM.Matrix  mtOT;
        SAPbouiCOM.Column clCode, clDescription, coVT, coVal, clIsActive, isNew, id, clMaxAllowed, clDays, clHours, clWeeks,clPerHour,clIsDefault, clFixValue, clDaysInYear, clIsFormula, clExpression, clPerDayCap, clPerMonthCap;
        SAPbouiCOM.EditText StrHours, StrWeeks, StrDay,strOtRatio;
        SAPbouiCOM.CheckBox cbValueType;
        bool flgPerHours;
        private SAPbouiCOM.DataTable dtOT;

        //**********************************

        //public IEnumerable<MstOverTime> overtimes;
        #endregion

        #region B1 Events

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.Freeze(true);
            InitiallizeForm();
            oForm.Freeze(false);

        }
        
        public override void etBeforeClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etBeforeClick(ref pVal, ref BubbleEvent);
            try
            {
                switch (pVal.ItemUID)
                {
                    case "1":
                        if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE || oForm.Mode == SAPbouiCOM.BoFormMode.fm_UPDATE_MODE)
                        {
                            if (!AddValidation())
                            {
                                BubbleEvent = false;
                            }
                        }
                        break;

                    //case "mtOT":
                    //    {
                    //        switch (pVal.ColUID)
                    //        {

                    //            case "isPerhrs":
                    //                flgPerHours = (mtOT.Columns.Item("isPerhrs").Cells.Item(pVal.Row).Specific as SAPbouiCOM.CheckBox).Checked;
                    //                if (flgPerHours == true)
                    //                {

                    //                    StrHours = mtOT.Columns.Item("clHours").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText;
                    //                    StrDay = mtOT.Columns.Item("clDays").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText;
                    //                    StrWeeks = mtOT.Columns.Item("clWeeks").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText;

                    //                    if (StrHours.Value == "0" || StrDay.Value == "0" || StrWeeks.Value == "0")
                    //                    {
                    //                        oApplication.StatusBar.SetText("Per Hour check box needs value in Hours,Days and weeks columns.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    //                        BubbleEvent = false;
                    //                        return;
                    //                    }

                    //                }
                    //                break;
                    //            //=================
                    //            case "clHours":
                    //                flgPerHours = (mtOT.Columns.Item("isPerhrs").Cells.Item(pVal.Row).Specific as SAPbouiCOM.CheckBox).Checked;
                    //                if (flgPerHours == true)
                    //                {

                    //                    StrHours = mtOT.Columns.Item("clHours").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText;
                    //                    StrDay = mtOT.Columns.Item("clDays").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText;
                    //                    StrWeeks = mtOT.Columns.Item("clWeeks").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText;
                    //                    if (StrHours.Value == "0")
                    //                    {
                    //                        oApplication.StatusBar.SetText("Per Hour check box needs value in Hours,Days and weeks columns.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    //                        BubbleEvent = false;
                    //                        return;
                    //                    }

                    //                }
                    //                break;
                    //            case "clDays":
                    //                flgPerHours = (mtOT.Columns.Item("isPerhrs").Cells.Item(pVal.Row).Specific as SAPbouiCOM.CheckBox).Checked;
                    //                if (flgPerHours == true)
                    //                {

                    //                    StrHours = mtOT.Columns.Item("clHours").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText;
                    //                    StrDay = mtOT.Columns.Item("clDays").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText;
                    //                    StrWeeks = mtOT.Columns.Item("clWeeks").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText;

                    //                    if (StrDay.Value == "0")
                    //                    {
                    //                        oApplication.StatusBar.SetText("Per Hour check box needs value in Hours,Days and weeks columns.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    //                        BubbleEvent = false;
                    //                        return;
                    //                    }

                    //                }
                    //                break;
                    //            case "clWeeks":
                    //                flgPerHours = (mtOT.Columns.Item("isPerhrs").Cells.Item(pVal.Row).Specific as SAPbouiCOM.CheckBox).Checked;
                    //                if (flgPerHours == true)
                    //                {

                    //                    StrHours = mtOT.Columns.Item("clHours").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText;
                    //                    StrDay = mtOT.Columns.Item("clDays").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText;
                    //                    StrWeeks = mtOT.Columns.Item("clWeeks").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText;

                    //                        if (StrWeeks.Value == "0")
                    //                        {
                    //                            oApplication.StatusBar.SetText("Per Hour check box needs value in Hours,Days and weeks columns.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    //                            BubbleEvent = false;
                    //                            return;
                    //                        }
                                        
                    //                }
                    //                break;
                    //        }

                    //    }
                    //    break;

                }
                

            }            
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return;
            }

        }
        
        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "1":
                    SaveRecords();
                    break;
            }
        }
        
        public override void etAfterLostFocus(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterLostFocus(ref pVal, ref BubbleEvent);
            BubbleEvent = true;
            try
            {

                //if (pVal.ColUID == "coCode")
                //{
                //    string Codevalue = (mtOT.Columns.Item("coCode").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                //    var OtCode = (from a in dbHrPayroll.MstOverTime where a.Code == Codevalue select a).FirstOrDefault();
                //    if (OtCode != null)
                //    {
                //        oApplication.StatusBar.SetText("Duplication in overtime master code.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                //        (mtOT.Columns.Item("coCode").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "";
                //        BubbleEvent = false;
                //    }
                //}
                if (pVal.ColUID == "coVal")
                {
                    string value = (mtOT.Columns.Item("coVal").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                    if (value.StartsWith("-"))
                    {
                        oApplication.StatusBar.SetText("Negetive value not allowed.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        (mtOT.Columns.Item("coVal").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "0.0000";
                        BubbleEvent = false;
                    }
                }
                if (pVal.ColUID == "clHours")
                {
                    string value = (mtOT.Columns.Item("clHours").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                    if (value.StartsWith("-"))
                    {
                        oApplication.StatusBar.SetText("Negetive value not allowed.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        (mtOT.Columns.Item("clHours").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "0.0";
                        BubbleEvent = false;
                    }
                }
                if (pVal.ColUID == "clDays")
                {
                    string value = (mtOT.Columns.Item("clDays").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                    if (value.StartsWith("-"))
                    {
                        oApplication.StatusBar.SetText("Negetive value not allowed.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        (mtOT.Columns.Item("clDays").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "0";
                        BubbleEvent = false;
                    }
                }
                if (pVal.ColUID == "clWeeks")
                {
                    string value = (mtOT.Columns.Item("clWeeks").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                    if (value.StartsWith("-"))
                    {
                        oApplication.StatusBar.SetText("Negetive value not allowed.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        (mtOT.Columns.Item("clWeeks").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "0";
                        BubbleEvent = false;
                    }
                }
                if (pVal.ColUID == "V_1")
                {
                    string value = (mtOT.Columns.Item("V_1").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                    if (value.StartsWith("-"))
                    {
                        oApplication.StatusBar.SetText("Negetive value not allowed.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        (mtOT.Columns.Item("V_1").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "0.0000";
                        BubbleEvent = false;
                    }
                }
                if (pVal.ColUID == "Max")
                {
                    string value = (mtOT.Columns.Item("Max").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                    if (value.StartsWith("-"))
                    {
                        oApplication.StatusBar.SetText("Negetive value not allowed.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        (mtOT.Columns.Item("Max").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "0.0000";
                        BubbleEvent = false;
                    }
                }
                if (pVal.ColUID == "V_0")
                {
                    string value = (mtOT.Columns.Item("V_0").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                    if (value.StartsWith("-"))
                    {
                        oApplication.StatusBar.SetText("Negetive value not allowed.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        (mtOT.Columns.Item("V_0").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "0.0000";
                        BubbleEvent = false;
                    }
                }
                if (pVal.ColUID == "coActive")
                {
                    string Codevalue = (mtOT.Columns.Item("coCode").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value;
                    var OtAttached = (from a in dbHrPayroll.MstOverTime where a.Code == Codevalue select a).FirstOrDefault();
                    if (OtAttached != null)
                    {
                        var ExistInSalary = (from a in dbHrPayroll.TrnsSalaryProcessRegisterDetail where a.LineBaseEntry == OtAttached.ID && a.LineType == "Over Time" select a).FirstOrDefault();
                        if (ExistInSalary != null)
                        {
                            oApplication.StatusBar.SetText("You can't deactive overtime it's process in salary.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            //(mtOT.Columns.Item("coActive").Cells.Item(pVal.Row).Specific as SAPbouiCOM.EditText).Value = "0.0000";
                            BubbleEvent = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {             
            
            }
        }

       
        #endregion

        #region Functions
        
        private void InitiallizeForm()
        {
            try
            {
                mtOT = oForm.Items.Item("mtOT").Specific;
                dtOT = oForm.DataSources.DataTables.Item("dtOT");
                isNew = mtOT.Columns.Item("isNew");
                id = mtOT.Columns.Item("id");
                isNew.Visible = false;
                id.Visible = false;
                clCode = mtOT.Columns.Item("coCode");
                clDescription = mtOT.Columns.Item("coDesc");
                clIsActive = mtOT.Columns.Item("coActive");
                clIsDefault = mtOT.Columns.Item("flgDflt");
                clIsDefault.Visible = false;
                clFixValue = mtOT.Columns.Item("fixv");
                clDaysInYear = mtOT.Columns.Item("diy");
                coVal = mtOT.Columns.Item("coVal");
                coVT = mtOT.Columns.Item("coVT");
                clMaxAllowed = mtOT.Columns.Item("Max");
                clDays = mtOT.Columns.Item("clDays");
                clHours = mtOT.Columns.Item("clHours");
                clWeeks = mtOT.Columns.Item("clWeeks");
                clPerHour = mtOT.Columns.Item("isPerhrs");
                clIsFormula = mtOT.Columns.Item("isfor");
                clExpression = mtOT.Columns.Item("expr");
                clPerDayCap = mtOT.Columns.Item("clpdc");
                clPerMonthCap = mtOT.Columns.Item("clpmc");
                FillGrid();
                fillColumCombo("Val_Type", coVT);
                mtOT.AutoResizeColumns();
            }
            catch(Exception ex)
            {
                Program.objHrmsUI.logger.LogException(Program.objHrmsUI.AppVersion, ex);
            }
        }
        
        private void FillGrid()
        {
            try
            {
                var OTCollection = (from a in dbHrPayroll.MstOverTime select a).ToList();
                dtOT.Rows.Clear();
                dtOT.Rows.Add(OTCollection.Count());
                int i = 0;
                foreach (MstOverTime OTLine in OTCollection)
                {
                    dtOT.SetValue("isNew", i, "N");
                    dtOT.SetValue("id", i, OTLine.ID);
                    dtOT.SetValue("OTCode", i, OTLine.Code.ToString());
                    dtOT.SetValue("Descr", i, OTLine.Description.ToString());
                    dtOT.SetValue("ValType", i, OTLine.ValueType.ToString());
                    dtOT.SetValue("Val", i, OTLine.Value.ToString());
                    dtOT.SetValue("Max", i, OTLine.MaxHour.ToString());
                    if (string.IsNullOrEmpty(OTLine.Hours))
                    {
                        dtOT.SetValue("clHours", i, "0.00");
                    }
                    else
                    {
                        dtOT.SetValue("clHours", i, OTLine.Hours.ToString());
                    }
                    if (string.IsNullOrEmpty(OTLine.Days))
                    {
                        dtOT.SetValue("clDays", i, "0");
                    }
                    else
                    {
                        dtOT.SetValue("clDays", i, OTLine.Days.ToString());
                    }
                    if (string.IsNullOrEmpty(OTLine.Weeks))
                    {
                        dtOT.SetValue("clWeeks", i, "0");
                    }
                    else
                    {
                        dtOT.SetValue("clWeeks", i, OTLine.Weeks.ToString());
                    }
                    if (OTLine.FlgDefault != null)
                    {
                        dtOT.SetValue("flgDflt", i, OTLine.FlgDefault == true ? "Y" : "N");
                    }
                    else
                    {
                        dtOT.SetValue("flgDflt", i, "N");
                    }
                    if (OTLine.FixValue != null)
                    {
                        dtOT.SetValue("FixV", i, OTLine.FixValue.ToString());
                    }
                    else
                    {
                        dtOT.SetValue("FixV", i, "0");
                    }
                    if (OTLine.DaysinYear != null)
                    {
                        dtOT.SetValue("DinY", i, OTLine.DaysinYear.ToString());
                    }
                    else
                    {
                        dtOT.SetValue("DinY", i, "0");
                    }
                    dtOT.SetValue("isPerhrs", i, OTLine.FlgPerHour == true ? "Y" : "N");
                    dtOT.SetValue("Active", i, OTLine.FlgActive == true ? "Y" : "N");
                    dtOT.SetValue(clIsFormula.DataBind.Alias, i, OTLine.FlgFormula == true ? "Y" : "N");
                    if (!string.IsNullOrEmpty(OTLine.Expression))
                    {
                        dtOT.SetValue(clExpression.DataBind.Alias, i, OTLine.Expression);
                    }
                    else
                    {
                        dtOT.SetValue(clExpression.DataBind.Alias, i, "");
                    }
                    if(OTLine.PerDayCap != null)
                    {
                        dtOT.SetValue(clPerDayCap.DataBind.Alias, i, Convert.ToDouble(OTLine.PerDayCap));
                    }
                    else
                    {
                        dtOT.SetValue(clPerDayCap.DataBind.Alias, i, 0);
                    }
                    if (OTLine.PerMonthCap != null)
                    {
                        dtOT.SetValue(clPerMonthCap.DataBind.Alias, i, Convert.ToDouble(OTLine.PerMonthCap));
                    }
                    else
                    {
                        dtOT.SetValue(clPerMonthCap.DataBind.Alias, i, 0);
                    }
                    i++;
                }
                AddEmptyRow();
                mtOT.LoadFromDataSource();
            }
            catch (Exception ex)
            {
                Program.objHrmsUI.logger.LogException(Program.objHrmsUI.AppVersion, ex);
            }
        }
        
        private void AddEmptyRow()
        {
            try
            {
                if (dtOT.Rows.Count == 0)
                {
                    dtOT.Rows.Add(1);

                    dtOT.SetValue("isNew", 0, "Y");
                    dtOT.SetValue("id", 0, 0);
                    dtOT.SetValue("OTCode", 0, "");
                    dtOT.SetValue("Descr", 0, "");
                    dtOT.SetValue("ValType", 0, "");
                    dtOT.SetValue("Val", 0, "0.00");
                    dtOT.SetValue("clHours", 0, "0.00");
                    dtOT.SetValue("clDays", 0, "0");
                    dtOT.SetValue("clWeeks", 0, "0");
                    dtOT.SetValue("Active", 0, "N");
                    dtOT.SetValue("flgDflt", 0, "N");
                    dtOT.SetValue("isPerhrs", 0, "N");
                    dtOT.SetValue(clIsFormula.DataBind.Alias, 0, "N");
                    dtOT.SetValue(clExpression.DataBind.Alias, 0, "");
                    dtOT.SetValue(clPerDayCap.DataBind.Alias, 0, 0);
                    dtOT.SetValue(clPerMonthCap.DataBind.Alias, 0, 0);
                    mtOT.AddRow(1, mtOT.RowCount + 1);
                }
                else
                {
                    if (dtOT.GetValue("OTCode", dtOT.Rows.Count - 1) == "")
                    {
                    }
                    else
                    {
                        dtOT.Rows.Add(1);
                        dtOT.SetValue("isNew", dtOT.Rows.Count - 1, "Y");
                        dtOT.SetValue("id", dtOT.Rows.Count - 1, 0);
                        dtOT.SetValue("OTCode", dtOT.Rows.Count - 1, "");
                        dtOT.SetValue("Descr", dtOT.Rows.Count - 1, "");
                        dtOT.SetValue("ValType", dtOT.Rows.Count - 1, "");
                        dtOT.SetValue("Val", dtOT.Rows.Count - 1, "0.00");
                        dtOT.SetValue("clHours", dtOT.Rows.Count - 1, "0.00");
                        dtOT.SetValue("clDays", dtOT.Rows.Count - 1, "0");
                        dtOT.SetValue("clWeeks", dtOT.Rows.Count - 1, "0");
                        dtOT.SetValue("Active", dtOT.Rows.Count - 1, "N");
                        dtOT.SetValue("flgDflt", dtOT.Rows.Count - 1, "N");
                        dtOT.SetValue("isPerhrs", dtOT.Rows.Count - 1, "N");
                        dtOT.SetValue(clIsFormula.DataBind.Alias, dtOT.Rows.Count - 1, "N");
                        dtOT.SetValue(clExpression.DataBind.Alias, dtOT.Rows.Count - 1, "");
                        dtOT.SetValue(clPerDayCap.DataBind.Alias, dtOT.Rows.Count - 1, 0);
                        dtOT.SetValue(clPerMonthCap.DataBind.Alias, dtOT.Rows.Count - 1, 0);
                        mtOT.AddRow(1, mtOT.RowCount + 1);
                    }

                }
                mtOT.LoadFromDataSource();
            }
            catch (Exception ex)
            {
                Program.objHrmsUI.logger.LogException(Program.objHrmsUI.AppVersion, ex);
            }
        }

        private void SaveRecords()
        {
            mtOT.FlushToDataSource();
            string id = "";
            string code = "";
            string isnew = "";
            string valueType = "";
            try
            {
                for (int i = 0; i < dtOT.Rows.Count; i++)
                {
                    code = Convert.ToString(dtOT.GetValue("OTCode", i));
                    isnew = Convert.ToString(dtOT.GetValue("isNew", i));
                    valueType = Convert.ToString(dtOT.GetValue("ValType", i));
                    isnew = isnew.Trim();
                    code = code.Trim();
                    if (code != "")
                    {
                        MstOverTime obj;
                        id = Convert.ToString(dtOT.GetValue("id", i));
                        if (isnew == "Y")
                        {
                            var oOT = (from a in dbHrPayroll.MstOverTime where a.Code == code select a).FirstOrDefault();
                            if (oOT != null)
                            {
                                oApplication.StatusBar.SetText("Duplication in overtime master code.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                return ;
                            }
                            obj = new MstOverTime();
                            dbHrPayroll.MstOverTime.InsertOnSubmit(obj);

                        }
                        else
                        {
                            obj = (from p in dbHrPayroll.MstOverTime where p.ID.ToString() == id.Trim() select p).FirstOrDefault();
                        }
                        obj.Code = dtOT.GetValue("OTCode", i);
                        obj.Description = dtOT.GetValue("Descr", i);
                        obj.ValueType = valueType; //Convert.ToString( dtOT.GetValue("ValType", i));
                        string va = Convert.ToString( dtOT.GetValue("Val", i));
                        obj.Value = Convert.ToDecimal( va);
                        obj.MaxHour = Convert.ToDecimal( dtOT.GetValue("Max", i));
                        obj.Hours = Convert.ToString(dtOT.GetValue("clHours", i));
                        obj.Days = Convert.ToString(dtOT.GetValue("clDays", i));
                        obj.Weeks = Convert.ToString(dtOT.GetValue("clWeeks", i));
                        obj.FixValue = Convert.ToDecimal(dtOT.GetValue("FixV", i));
                        obj.DaysinYear = Convert.ToDecimal(dtOT.GetValue("DinY", i)); 
                        obj.FlgActive = Convert.ToString(dtOT.GetValue("Active", i)) == "Y" ? true : false;
                        obj.FlgDefault = Convert.ToString(dtOT.GetValue("flgDflt", i)) == "Y" ? true : false;
                        obj.FlgPerHour = Convert.ToString(dtOT.GetValue("isPerhrs", i)) == "Y" ? true : false;
                        obj.FlgFormula = Convert.ToString(dtOT.GetValue(clIsFormula.DataBind.Alias, i)) == "Y" ? true : false;
                        obj.Expression = Convert.ToString(dtOT.GetValue(clExpression.DataBind.Alias, i));
                        obj.PerDayCap = Convert.ToDecimal(dtOT.GetValue(clPerDayCap.DataBind.Alias, i));
                        obj.PerMonthCap = Convert.ToDecimal(dtOT.GetValue(clPerMonthCap.DataBind.Alias, i));
                        obj.CreateDate = DateTime.Now;
                        obj.UserId = oCompany.UserName;
                        obj.UpdateDate = DateTime.Now;
                        obj.UpdatedBy = oCompany.UserName;
                    }
                }
                dbHrPayroll.SubmitChanges();
                FillGrid();
                //addEmptyRow();
            }
            catch (Exception ex)
            {
                //oApplication.StatusBar.SetText("Form: frm_OTSetup Function: updateDbWithMat Msg: " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                Program.objHrmsUI.logger.LogException(Program.objHrmsUI.AppVersion, ex);
            }
        }
        
        private Boolean AddValidation()
        {
            try
            {
                mtOT.FlushToDataSource();
                for (int i = 0; i < dtOT.Rows.Count; i++)
                {
                    string Overtimecode = dtOT.GetValue(clCode.DataBind.Alias, i);
                    string OverTimetatus = dtOT.GetValue(clIsActive.DataBind.Alias, i);
                    string StrHours = dtOT.GetValue(clHours.DataBind.Alias, i);
                    string strWeeks = dtOT.GetValue(clWeeks.DataBind.Alias, i);
                    string StrDays = dtOT.GetValue(clDays.DataBind.Alias, i);
                    string flgPerhour = dtOT.GetValue(clPerHour.DataBind.Alias, i);
                    string flgFormula = dtOT.GetValue(clIsFormula.DataBind.Alias, i);
                    string FormulaExpression = dtOT.GetValue(clExpression.DataBind.Alias, i);

                    if (!string.IsNullOrEmpty(Overtimecode) && !string.IsNullOrEmpty(OverTimetatus))
                    {
                        if (flgPerhour.Trim().ToLower() == "y")
                        {
                            if (strWeeks == "0" || strWeeks == "0.0")
                            {
                                oApplication.StatusBar.SetText("Per Hour check box needs value in Hours, Days and weeks columns.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                return false;
                            }
                            if (StrHours == "0" || StrHours == "0.0")
                            {
                                oApplication.StatusBar.SetText("Per Hour check box needs value in Hours, Days and weeks columns.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                return false;
                            }
                            if (StrDays == "0" || StrDays == "0.0")
                            {
                                oApplication.StatusBar.SetText("Per Hour check box needs value in Hours, Days and weeks columns.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                return false;
                            }
                            if (StrDays == "0" || StrDays == "0.0")
                            {
                                oApplication.StatusBar.SetText("Per Hour check box needs value in Hours, Days and weeks columns.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                return false;
                            }
                            if (flgFormula.Trim().ToLower() == "n")
                            {
                                oApplication.StatusBar.SetText("Per Hour check box needs value in Hours, Days and weeks columns.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                return false;
                            }
                            if (flgFormula.Trim().ToLower() == "y")
                            {
                                if (FormulaExpression == "")
                                {
                                    oApplication.StatusBar.SetText("Please fill formula Expression column if checked Is formula column.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    return false;
                                }
                            }
                        }

                        if (flgFormula.Trim().ToLower() == "y")
                        {
                            if (FormulaExpression == "")
                            {
                                oApplication.StatusBar.SetText("Please fill formula Expression column if checked Is formula column.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                return false;
                            }
                        }

                        Boolean active = false;
                        var oOT = (from a in dbHrPayroll.MstOverTime where a.Code == Overtimecode select a).FirstOrDefault();
                        if (oOT == null) continue;
                        //if (oOT != null)
                        //{
                        //    oApplication.StatusBar.SetText("Duplication in overtime master code.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        //    return false;
                        //}
                        if (OverTimetatus.Trim().ToLower() == "y")
                        {
                            active = true;
                        }
                        if (active != Convert.ToBoolean(oOT.FlgActive))
                        {
                            var ExistInSalary = (from a in dbHrPayroll.TrnsSalaryProcessRegisterDetail where a.LineBaseEntry == oOT.ID && a.LineType == "Over Time" select a).FirstOrDefault();
                            if (ExistInSalary != null)
                            {
                                oApplication.StatusBar.SetText("You can't inactive overtime when it's processed in salary.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                return false;
                            }
                        }
                    }

                }
                return true;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("AddValidation : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                return false;
            }
        }
        
        #endregion
    }
}
