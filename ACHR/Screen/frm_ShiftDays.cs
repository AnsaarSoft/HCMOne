using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using DIHRMS.Custom;
using System.Data;
using SAPbobsCOM;
using SAPbouiCOM;

namespace ACHR.Screen
{
    class frm_ShiftDays : HRMSBaseForm
    {
        #region Variables

        Button btnMain, btnCancel, btnVerify;
        Item ibtnMain, ibtnCancel, ibtnVerify;
        Matrix grdMain;
        SAPbouiCOM.DataTable dtMain;
        Column clSerial, clCode, clDesc, clDays, clIsNew, clID;

        #endregion

        #region B1 Events

        public override void CreateForm(Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            try
            {
                oForm.Freeze(true);
                InitiallizeForm();
                oForm.Freeze(false);
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        public override void etBeforeClick(ref ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etBeforeClick(ref pVal, ref BubbleEvent);
            if (pVal.ItemUID == ibtnMain.UniqueID)
            {
                if (!ValidateGrid())
                {
                    BubbleEvent = false;
                }
            }
        }

        public override void etAfterClick(ref ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            if (pVal.ItemUID == ibtnMain.UniqueID)
            {
                if (SubmitRecords())
                {
                    GetAllData();
                    AddEmptyRow();
                }
            }
            else if (pVal.ItemUID == ibtnVerify.UniqueID)
            {
                VerifyAllEmployeeShiftDays();
            }
        }

        #endregion

        #region Functions

        private void InitiallizeForm()
        {
            try
            {
                btnMain = oForm.Items.Item("1").Specific;
                ibtnMain = oForm.Items.Item("1");

                btnCancel = oForm.Items.Item("2").Specific;
                ibtnCancel = oForm.Items.Item("2");

                btnVerify = oForm.Items.Item("btverify").Specific;
                ibtnVerify = oForm.Items.Item("btverify");

                grdMain = oForm.Items.Item("mtmain").Specific;
                dtMain = oForm.DataSources.DataTables.Item("dtmain");
                clIsNew = grdMain.Columns.Item("clisnew");
                clIsNew.Visible = false;
                clID = grdMain.Columns.Item("clid");
                clID.Visible = false;
                clCode = grdMain.Columns.Item("clcode");
                clCode.TitleObject.Sortable = false;
                clDesc = grdMain.Columns.Item("cldesc");
                clDesc.TitleObject.Sortable = false;
                clDays = grdMain.Columns.Item("cldays");
                clDays.TitleObject.Sortable = false;
                clSerial = grdMain.Columns.Item("clno");
                //flgActive.TitleObject.Sortable = false;

                grdMain.AutoResizeColumns();
                GetAllData();
                AddEmptyRow();
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        private void GetAllData()
        {
            try
            {
                var oCollection = (from a in dbHrPayroll.MstShiftDays
                                   select a).ToList();
                dtMain.Rows.Clear();
                dtMain.Rows.Add(oCollection.Count());
                Int32 i = 0;
                foreach (var One in oCollection)
                {
                    dtMain.SetValue(clIsNew.DataBind.Alias, i, "N");
                    dtMain.SetValue(clID.DataBind.Alias, i, One.InternalID);
                    dtMain.SetValue(clCode.DataBind.Alias, i, One.Code);
                    dtMain.SetValue(clDesc.DataBind.Alias, i, One.Description);
                    dtMain.SetValue(clDays.DataBind.Alias, i, One.DaysCount);
                    dtMain.SetValue(clSerial.DataBind.Alias, i, i + 1);
                    i++;
                }
            }
            catch (Exception ex)
            {
                logger(ex);
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
                dtMain.SetValue(clCode.DataBind.Alias, RowValue - 1, "");
                dtMain.SetValue(clDesc.DataBind.Alias, RowValue - 1, "");
                dtMain.SetValue(clDays.DataBind.Alias, RowValue - 1, "0");
                dtMain.SetValue(clSerial.DataBind.Alias, RowValue - 1, RowValue);
                grdMain.AddRow(1, RowValue + 1);
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
                    dtMain.SetValue(clIsNew.DataBind.Alias, RowValue - 1, "Y");
                    dtMain.SetValue(clID.DataBind.Alias, RowValue - 1, "0");
                    dtMain.SetValue(clCode.DataBind.Alias, RowValue - 1, "");
                    dtMain.SetValue(clDesc.DataBind.Alias, RowValue - 1, "");
                    dtMain.SetValue(clDays.DataBind.Alias, RowValue - 1, "0");
                    dtMain.SetValue(clSerial.DataBind.Alias, RowValue - 1, RowValue);
                    grdMain.AddRow(1, grdMain.RowCount + 1);
                }
            }
            grdMain.LoadFromDataSource();
        }

        private bool ValidateGrid()
        {
            try
            {
                List<string> oList = new List<string>();
                grdMain.FlushToDataSource();
                for (int i = 0; i < dtMain.Rows.Count; i++)
                {
                    string code = Convert.ToString(dtMain.GetValue(clCode.DataBind.Alias, i));
                    if (!string.IsNullOrEmpty(code))
                    {
                        if (string.IsNullOrWhiteSpace(code))
                        {
                            MsgWarning("Code field can't contains whitespaces.");
                            return false;
                        }
                    }
                    if (!string.IsNullOrEmpty(code))
                    {
                        oList.Add(code);
                    }
                }
                if (oList.Count != oList.Distinct().Count())
                {
                    MsgWarning("You can't have duplicate Codes.");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                logger(ex);
                return false;
            }
        }

        private bool SubmitRecords()
        {
            try
            {
                grdMain.FlushToDataSource();
                for (int i = 0; i < dtMain.Rows.Count; i++)
                {
                    string code, desc, days, isnew, id;
                    id = Convert.ToString(dtMain.GetValue(clID.DataBind.Alias, i));
                    isnew = Convert.ToString(dtMain.GetValue(clIsNew.DataBind.Alias, i));
                    code = Convert.ToString(dtMain.GetValue(clCode.DataBind.Alias, i));
                    desc = Convert.ToString(dtMain.GetValue(clDesc.DataBind.Alias, i));
                    days = Convert.ToString(dtMain.GetValue(clDays.DataBind.Alias, i));
                    if (string.IsNullOrEmpty(code)) continue;
                    if (isnew == "Y")
                    {
                        MstShiftDays oNew = new MstShiftDays();
                        dbHrPayroll.MstShiftDays.InsertOnSubmit(oNew);
                        oNew.Code = code;
                        oNew.Description = desc;
                        oNew.DaysCount = Convert.ToInt32(days);
                        oNew.CreatedBy = oCompany.UserName;
                        oNew.UpdatedBy = oCompany.UserName;
                        oNew.CreateDate = DateTime.Now;
                        oNew.UpdateDate = DateTime.Now;
                    }
                    else
                    {

                        MstShiftDays oDoc = (from a in dbHrPayroll.MstShiftDays where a.InternalID.ToString() == id select a).FirstOrDefault();
                        oDoc.Code = code;
                        oDoc.Description = desc;
                        oDoc.DaysCount = Convert.ToInt32(days);
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

        private void VerifyAllEmployeeShiftDays()
        {
            try
            {
                var oEmpCollection = (from a in dbHrPayroll.MstEmployee
                                      where a.FlgActive == true
                                      && a.ShiftDaysCode != null
                                      select a).ToList();
                foreach (var One in oEmpCollection)
                {
                    var oShiftDays = (from a in dbHrPayroll.MstShiftDays
                                      where a.Code == One.ShiftDaysCode
                                      select a).FirstOrDefault();
                    var oCurrentFiscal = (from a in dbHrPayroll.MstCalendar
                                          where a.FlgActive == true
                                          select a).FirstOrDefault();
                    DateTime StartDate, EndDate;
                    StartDate = Convert.ToDateTime(One.JoiningDate);
                    EndDate = Convert.ToDateTime(oCurrentFiscal.EndDate);
                    bool flgOnOff = true;
                    int BaseDocDays = Convert.ToInt32(oShiftDays.DaysCount);
                    int RunningValue = 1;
                    for (DateTime Running = StartDate; Running <= EndDate; Running = Running.AddDays(1))
                    {
                        var oCount = (from a in dbHrPayroll.TrnsShiftsDaysRegister
                                      where a.RecordDate == Running
                                      && a.EmpCode == One.EmpID
                                      select a).Count();
                        if (oCount > 0)
                        {
                            TrnsShiftsDaysRegister oNew = (from a in dbHrPayroll.TrnsShiftsDaysRegister
                                                           where a.RecordDate == Running
                                                           && a.EmpCode == One.EmpID
                                                           select a).FirstOrDefault();
                            oNew.EmpCode = One.EmpID;
                            oNew.EmpName = One.FirstName + " " + One.MiddleName + " " + One.LastName;
                            oNew.ShiftName = oShiftDays.Code;
                            oNew.RecordDate = Running;
                            if (flgOnOff)
                            {
                                oNew.DayStatus = 1;
                            }
                            else
                            {
                                oNew.DayStatus = 0;
                            }
                            oNew.UpdatedBy = oCompany.UserName;
                            oNew.UpdateDate = DateTime.Now;
                        }
                        else
                        {
                            TrnsShiftsDaysRegister oNew = new TrnsShiftsDaysRegister();
                            dbHrPayroll.TrnsShiftsDaysRegister.InsertOnSubmit(oNew);
                            oNew.EmpCode = One.EmpID;
                            oNew.EmpName = One.FirstName + " " + One.MiddleName + " " + One.LastName;
                            oNew.ShiftName = oShiftDays.Code;
                            oNew.RecordDate = Running;
                            if (flgOnOff)
                            {
                                oNew.DayStatus = 1;
                            }
                            else
                            {
                                oNew.DayStatus = 0;
                            }
                            oNew.CreatedBy = oCompany.UserName;
                            oNew.UpdatedBy = oCompany.UserName;
                            oNew.CreateDate = DateTime.Now;
                            oNew.UpdateDate = DateTime.Now;
                        }
                        if (RunningValue < BaseDocDays)
                        {
                            RunningValue++;
                        }
                        else
                        {
                            RunningValue = 1;
                            if (flgOnOff)
                            {
                                flgOnOff = false;
                            }
                            else
                            {
                                flgOnOff = true;
                            }
                        }
                    }
                }
                dbHrPayroll.SubmitChanges();
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }

        #endregion

    }
}
